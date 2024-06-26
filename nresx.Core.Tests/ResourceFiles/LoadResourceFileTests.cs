﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using nresx.Tools;
using nresx.Tools.Helpers;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles
{
    [TestFixture]
    public class LoadResourceFileTests : TestBase
    {
        [Test]
        public async Task ParsePlainTxt()
        {
            var res = new ResourceFile( GetTestPath( "Resources.txt" ) );
            var targetPath = GetOutputPath( UniqueKey(), res.FileFormat );
            res.Save( targetPath );

            res = new ResourceFile( targetPath );

            res.FileName.Should().Be( Path.GetFileName( targetPath ) );
            res.AbsolutePath.Should().Be( Path.GetFullPath( targetPath ) );
            res.FileFormat.Should().Be( ResourceFormatType.PlainText );

            res.Elements.Select( el => (el.Key, el.Value) ).Should()
                .BeEquivalentTo( GetExampleResourceFile().Elements.Select( el => (el.Value, el.Value) ) );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public async Task ParsedResourceFileShouldContainsFileNameAndPath( string path )
        {
            var res = new ResourceFile( GetTestPath( path ) );
            var targetPath = GetOutputPath( UniqueKey(), res.FileFormat );
            res.Save( targetPath );

            res = new ResourceFile( targetPath );

            res.FileName.Should().Be( Path.GetFileName( targetPath ) );
            res.AbsolutePath.Should().Be( Path.GetFullPath( targetPath ) );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public async Task StreamParsedResourceFileShouldContainsFileNameAndPath( string path )
        {
            var res = new ResourceFile( GetTestPath( path ) );
            var targetPath = GetOutputPath( UniqueKey(), res.FileFormat );
            res.Save( targetPath );

            await using var stream = new FileStream( targetPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
            res = new ResourceFile( stream );

            res.FileName.Should().Be( Path.GetFileName( targetPath ) );
            res.AbsolutePath.Should().Be( Path.GetFullPath( targetPath ) );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public async Task LoadFromPath( string path )
        {
            ResourceFormatHelper.DetectFormatByExtension( path, out var targetType );
            var res = new ResourceFile( GetTestPath( path ) );

            res.FileFormat.Should().Be( targetType );
            ValidateElements( res );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public async Task LoadFromStream( string path )
        {
            ResourceFormatHelper.DetectFormatByExtension( path, out var targetType );
            await using var stream = new FileStream( GetTestPath( path ), FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
            var res = new ResourceFile( stream );

            res.FileFormat.Should().Be( targetType );
            ValidateElements( res );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFormats ) )]
        public async Task LoadRawElements( ResourceFormatType format )
        {
            var source = new ResourceFile( GetTestPath( TestData.ExampleResourceFile, format ) );
            var duplicated = GetOutputPath( TestData.UniqueKey(), format );
            TestHelper.CopyTemporaryFile( source.AbsolutePath, duplicated, copyType: format );

            TestHelper.ReplaceKey( duplicated, source.Elements[1].Key, source.Elements[0].Key );

            var elements = ResourceFile.LoadRawElements( duplicated );
            elements.Should().HaveCount( source.Elements.Count() );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFormats ) )]
        public async Task LoadRawElementsByStream( ResourceFormatType format )
        {
            var source = new ResourceFile( GetTestPath( TestData.ExampleResourceFile, format ) );
            var duplicated = GetOutputPath( TestData.UniqueKey(), format );
            TestHelper.CopyTemporaryFile( source.AbsolutePath, duplicated, copyType: format );

            await using var stream = new FileStream( duplicated, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );

            var elements = ResourceFile.LoadRawElements( stream );
            elements.Should().HaveCount( source.Elements.Count() );
        }
    }
}