﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using nresx.Tools;
using nresx.Tools.Extensions;
using nresx.Tools.Helpers;

namespace nresx.Core.Tests
{
    public class TestHelper
    {
        private static void ReplaceTags( 
            StringBuilder resultCmdLine, 
            string tagPlaceholder, 
            Func<ResourceFormatType, string, string, string> getTagValue)
        {
            const string dirPlaceholder = @"Dir\";
            var tagName = tagPlaceholder.TrimStart( ' ', '[' ).TrimEnd( ' ', ']' );
            //var regx = new Regex( $"\\[{tagName}(.[\\w]+|)\\]", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );
            var regx = new Regex( $"\\[(Dir\\\\|){tagName}(.[\\w]+|)\\]", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );
            var matches = regx.Matches( resultCmdLine.ToString() );

            foreach ( Match match in matches )
            {
                string tag;
                var formatType = ResourceFormatType.Resx;
                var param = "";
                var dir = "";
                var dirPrefix = match.Groups[1].Value;
                if ( match.Groups.Count > 1 && dirPrefix == dirPlaceholder )
                {
                    dir = $"{TestData.UniqueKey()}\\";
                    new DirectoryInfo( Path.Combine( TestData.OutputFolder, dir ) ).Create();
                }

                if ( match.Groups.Count > 2 && !string.IsNullOrWhiteSpace( match.Groups[2].Value ) )
                {
                    var ext = match.Groups[2].Value;
                    param = ext.TrimStart( '.' );
                    if ( ResourceFormatHelper.DetectFormatByExtension( ext, out var t ) )
                        formatType = t;
                    tag = $"[{dirPrefix}{tagName}{ext}]";
                }
                else
                {
                    tag = $"[{dirPrefix}{tagName}]";
                }

                for ( int p = 0, i = resultCmdLine.ToString().IndexOf( tag, StringComparison.InvariantCulture );
                    i >= 0;
                    p += i, i = resultCmdLine.ToString( p, resultCmdLine.Length - p ).IndexOf( tag, StringComparison.Ordinal ) )
                {
                    resultCmdLine.Replace( tag, getTagValue( formatType, dir, param ), p + i, tag.Length );
                }
            }
        }

        public static string GetTestPath( string fileName, ResourceFormatType type = ResourceFormatType.Resx )
        {
            var path = Path.Combine( TestData.TestFileFolder, fileName );
            if ( !Path.HasExtension( path ) && ResourceFormatHelper.DetectExtension( type, out var extension ) )
                path = Path.ChangeExtension( path, extension );
            return path;
        }

        public static string GetOutputPath( string fileName, ResourceFormatType type = ResourceFormatType.Resx )
        {
            var path = Path.Combine( TestData.OutputFolder, fileName );
            if ( !Path.HasExtension( path ) && ResourceFormatHelper.DetectExtension( type, out var extension ) )
                path = Path.ChangeExtension( path, extension );
            return path;
        }

        public static string CopyTemporaryFile(
            string sourcePath = null,
            string destPath = null,
            string destDir = null,
            ResourceFormatType copyType = ResourceFormatType.Resx )
        {
            var key = TestData.UniqueKey();
            if ( string.IsNullOrWhiteSpace( destPath ) )
            {
                var file = string.IsNullOrWhiteSpace( destDir ) ? key : Path.Combine( destDir, key );
                destPath = GetOutputPath( file, copyType );
            }

            var resx = new ResourceFile( sourcePath ?? GetTestPath( TestData.ExampleResourceFile ) );
            resx.Save( destPath, copyType, createDir: true );

            return destPath;
        }


        public static void ReplaceKey( string path, string key, string newValue )
        {
            var lines = new StringBuilder();
            using ( var reader = new StreamReader( new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) ) )
            {
                while ( !reader.EndOfStream )
                    lines.AppendLine( reader.ReadLine()?.Replace( key, newValue ) );
            }
            using var writer = new StreamWriter( new FileStream( path, FileMode.Create, FileAccess.Write, FileShare.Write ) );
            var content = new MemoryStream( Encoding.UTF8.GetBytes( lines.ToString() ) );
            using ( var reader = new StreamReader( content ) )
            {
                while ( !reader.EndOfStream )
                    writer.WriteLine( reader.ReadLine() );
            }
        }

        public static string PrepareCommandLine(
            string cmdLine,
            out CommandLineParameters parameters,
            CommandLineParameters predefinedParams = null,
            bool mergeArgs = false ) // temporary solution, eventually it must be default behavior
        {
            var resultParams = new CommandLineParameters();
            var resultCmdLine = new StringBuilder( cmdLine );

            // replace static params
            resultCmdLine
                .Replace( CommandLineTags.FilesDir, TestData.TestFileFolder )
                .Replace( CommandLineTags.OutputDir, TestData.OutputFolder );

            // get resource files and replace its paths
            ReplaceTags(
                resultCmdLine, CommandLineTags.SourceFile,
                ( type, dir, parameter ) =>
                {
                    if ( predefinedParams != null && predefinedParams.SourceFiles.TryTake( out var p ) )
                    {
                        if( mergeArgs )
                            resultParams.SourceFiles.Add( p );
                        return p;
                    }

                    var path = GetTestPath( Path.ChangeExtension( TestData.ExampleResourceFile, "" ), type );
                    resultParams.SourceFiles.Add( path );
                    return path;
                } );

            // generate output files paths and replace in command line
            ReplaceTags(
                resultCmdLine, CommandLineTags.NewFile,
                ( type, dir, parameter ) =>
                {
                    if ( predefinedParams != null && predefinedParams.NewFiles.TryTake( out var p ) )
                    {
                        if( mergeArgs )
                            resultParams.NewFiles.Add( p );
                        return p;
                    }

                    var file = string.IsNullOrWhiteSpace( dir ) ? TestData.UniqueKey() : Path.Combine( dir, TestData.UniqueKey() );
                    var path = GetOutputPath( file, type );
                    resultParams.NewFiles.Add( path );
                    return path;
                } );

            // generate temporary files and replace its paths
            ReplaceTags(
                resultCmdLine, CommandLineTags.TemporaryFile,
                ( type, dir, parameter ) =>
                {
                    if ( predefinedParams != null && predefinedParams.TemporaryFiles.TryTake( out var p ) )
                    {
                        if ( mergeArgs )
                            resultParams.TemporaryFiles.Add( p );
                        return p;
                    }

                    var destPath = CopyTemporaryFile( copyType: type, destDir: dir );
                    resultParams.TemporaryFiles.Add( destPath );
                    return destPath;
                } );

            // generate unique key(s) and replace in command line
            ReplaceTags(
                resultCmdLine, CommandLineTags.UniqueKey,
                ( type, dir, parameter ) =>
                {
                    if ( predefinedParams != null && predefinedParams.UniqueKeys.TryTake( out var p ) )
                    {
                        if( mergeArgs )
                            resultParams.UniqueKeys.Add( p );
                        return p;
                    }

                    var key = TestData.UniqueKey();
                    resultParams.UniqueKeys.Add( key );
                    return key;
                } );

            // create new directory
            ReplaceTags(
                resultCmdLine, CommandLineTags.NewDir,
                ( type, dir, parameter ) =>
                {
                    if ( predefinedParams != null && predefinedParams.NewDirectories.TryTake( out var p ) )
                    {
                        if( mergeArgs )
                            resultParams.NewDirectories.Add( p );
                        return p;
                    }

                    var newDir = string.IsNullOrWhiteSpace( dir ) ? $"{TestData.UniqueKey()}" : Path.Combine( dir, TestData.UniqueKey() );
                    var dirInfo = new DirectoryInfo( Path.Combine( TestData.OutputFolder, newDir ) );
                    if ( !dirInfo.Exists )
                        dirInfo.Create();

                    //var destPath = CopyTemporaryFile( copyType: type, destDir: dir );
                    resultParams.NewDirectories.Add( newDir );
                    return newDir;
                } );

            // create copy of the project in temporary output directory
            ReplaceTags(
                resultCmdLine, CommandLineTags.TemporaryProjectDir,
                ( type, dir, parameter ) =>
                {
                    if ( predefinedParams != null && predefinedParams.TemporaryProjects.TryTake( out var p ) )
                    {
                        if ( mergeArgs )
                            resultParams.TemporaryProjects.Add( p );
                        return p;
                    }

                    var projDir = Path.Combine( TestData.ProjectsFolder, parameter );
                    var targetDir = Path.Combine( TestData.OutputFolder, $"{parameter}_{TestData.UniqueKey()}" );
                    FilesHelper.CopyDirectory( projDir, targetDir );

                    resultParams.TemporaryProjects.Add( targetDir );
                    return targetDir;
                } );

            var result = resultCmdLine.ToString();

            resultParams.CommandLine = result;
            resultParams.DryRun = result.Contains( TestData.DryRunOption );
            resultParams.Recursive = result.Contains( TestData.RecursiveOption ) || result.Contains( TestData.RecursiveShortOption );

            parameters = resultParams;
            return result;
        }

        public static CommandLineParameters RunCommandLine( 
            string cmdLine, 
            CommandLineParameters parameters = null, 
            bool mergeArgs = false )
        {
            var args = PrepareCommandLine( cmdLine, out var p, parameters, mergeArgs );

            var cmd = $"/C nresx {args}";
            var process = new Process();
            process.StartInfo = new ProcessStartInfo( "CMD.exe", cmd );
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit( 5000 );

            p.ExitCode = process.ExitCode;

            while ( !process.StandardOutput.EndOfStream )
            {
                var line = process.StandardOutput.ReadLine();
                p.ConsoleOutput.Add( line );
            }
            
            return p;
        }
    }
}