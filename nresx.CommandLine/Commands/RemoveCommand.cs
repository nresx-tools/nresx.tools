﻿using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using nresx.CommandLine.Commands.Base;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "remove", HelpText = "Remove an element from the resource file" )]
    public class RemoveCommand : BaseCommand, ICommand
    {
        [Option( 'k', "key", HelpText = "element key" )]
        public IEnumerable<string> Keys { get; set; }


        [Option( "empty", HelpText = "Remove all empty elements - key or value" )]
        public bool Empty { get; set; }

        [Option( "empty-key", HelpText = "Remove all elements with empty key" )]
        public bool EmptyKey { get; set; }

        [Option( "empty-value", HelpText = "Remove all elements with empty value" )]
        public bool EmptyValue { get; set; }

        protected override bool IsRecursiveAllowed => true;

        //[Option( "duplicates", HelpText = "Remove all empty elements - key or value" )]
        //public bool Duplicates { get; set; }

        protected override void ExecuteCommand()
        {
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: true, multipleIndirect: true )
                .Validate();
            if ( !optionsParsed )
                return;

            ForEachSourceFile(
                sourceFiles,
                ( file, resource ) =>
                {
                    var shortFilePath = resource.AbsolutePath.GetShortPath();
                    var elementsToDelete = new List<string>();

                    // remove element by key
                    if ( Keys?.Count() > 0 )
                    {
                        elementsToDelete.AddRange( Keys );
                    }

                    // remove all elements with empty key
                    if ( EmptyKey || EmptyValue || Empty )
                    {
                        elementsToDelete.AddRange( resource
                            .Elements
                            .Where( element =>
                                ( ( EmptyKey || Empty ) && string.IsNullOrWhiteSpace( element.Key ) ) ||
                                ( ( EmptyValue || Empty ) && string.IsNullOrWhiteSpace( element.Value ) ) )
                            .Select( element => element.Key ) );
                    }

                    foreach ( var key in elementsToDelete )
                    {
                        if ( !DryRun )
                        {
                            resource.Elements.Remove( key );
                        }
                        Console.WriteLine( $"{shortFilePath}: '{key}' have been removed" );
                    }

                    if ( !DryRun && !resource.IsNewFile )
                    {
                        resource.Save( resource.AbsolutePath );
                    }
                });
        }
    }
}