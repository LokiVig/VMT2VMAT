using System.Diagnostics;

namespace VMT2VMAT;

/// <summary>
/// The main program.<br/>
/// Everything to do with translating VMTs to VMATs is done here.
/// </summary>
public class Program
{
    /// <summary>
    /// Which version of Source 2 we should parse to.
    /// </summary>
    private static EngineVersion version = EngineVersion.HLA;

    /// <summary>
    /// Which file type we should use for textures.
    /// </summary>
    private static string fileExtension = "tga";

    /// <summary>
    /// The path to the VMT file we want to translate.
    /// </summary>
    private static string inputFile = string.Empty;

    /// <summary>
    /// The path to the VMAT file we want to translate.
    /// </summary>
    private static string outputFile = string.Empty;

    /// <summary>
    /// Path to a folder we should use for bulk-translation.
    /// </summary>
    private static string inputFolder = string.Empty;

    /// <summary>
    /// Path to a folder we should use to save the translated files to.
    /// </summary>
    private static string outputFolder = string.Empty;

    /// <summary>
    /// Does everything necessary to actually translate a VMT to a VMAT.
    /// </summary>
    /// <param name="args">The user's input arguments.</param>
    public static void Main( string[] args )
    {
        // Check every argument
        for ( int i = 0; i < args.Length; i++ )
        {
            // If we're passing the path to a VMT file...
            if ( IsValidArg( args[i], "-input" ) )
            {
                // If the argument features an extension...
                if ( Path.HasExtension( args[i + 1] ) )
                {
                    // Only translate the lone file!
                    inputFile = args[i + 1];
                }
                else // Otherwise...
                {
                    // We're specifying a folder!
                    inputFolder = args[i + 1];
                }
            }

            // If we're passing the path for the output VMAT...
            if ( IsValidArg( args[i], "-output" ) )
            {
                // If the argument features an extension...
                if ( Path.HasExtension( args[i + 1] ) )
                {
                    // Only translate the lone file!
                    outputFile = args[i + 1];
                }
                else // Otherwise...
                {
                    // We're specifying a folder!
                    outputFolder = args[i + 1];
                }
            }

            // If we're specifying a version...
            if ( IsValidArg( args[i], "-version" ) )
            {
                if ( IsValidArg( args[i + 1], "hla" ) ) // Half-Life: Alyx
                {
                    version = EngineVersion.HLA;
                }
                else if ( IsValidArg( args[i + 1], "cs2" ) ) // Counter Strike 2
                {
                    version = EngineVersion.CS2;
                }
                else if ( IsValidArg( args[i + 1], "sbox" ) ) // s&box
                {
                    version = EngineVersion.Sandbox;
                }
                else // Assume invalid input! Default to HL:A
                {
                    Console.Error.WriteLine( "Invalid Source 2 version provided! Defaulting to HLA..." );
                    version = EngineVersion.HLA;
                }
            }

            // If we're specifying the file extension for our textures...
            if ( IsValidArg( args[i], "-fileextension" ) )
            {
                if ( IsValidArg( args[i + 1], "tga" ) ) // TGA
                {
                    fileExtension = "tga";
                }
                else if ( IsValidArg( args[i + 1], "png" ) ) // PNG
                {
                    fileExtension = "png";
                }
                else if ( IsValidArg( args[i + 1], "jpg" )
                    || IsValidArg( args[i + 1], "jpeg" ) ) // JPG
                {
                    fileExtension = "jpg";
                }
                else // Assume invalid input! Default to TGA
                {
                    Console.Error.WriteLine( "Invalid file extension provided! Defaulting to TGA..." );
                    fileExtension = "tga";
                }
            }
        }

        // If we're passing a VMT file and a folder...
        if ( !string.IsNullOrEmpty( inputFile ) && !string.IsNullOrEmpty( inputFolder ) )
        {
            // Tell the user it's not possible!
            Console.Error.WriteLine( "Can't specify a VMT path AND folder!" );
            return;
        }

        // Check if the provided path to a VMT is valid...
        if ( !IsValidVMT( inputFile ) && !Directory.Exists( inputFolder ) )
        {
            // If not, whoopsie!
            Console.Error.WriteLine( "Invalid VMT file / folder given!" );
            return;
        }

        // If the input folder is a valid directory...
        if ( Directory.Exists( inputFolder ) )
        {
            string[] files = Directory.GetFiles( inputFolder, "*.vmt", SearchOption.AllDirectories );
            int fileCount = files.Length; // The total amount of files
            int errCount = 0; // The total amount of errors

            // For every file in this folder and all recursive folders...
            for ( int i = 0; i < fileCount; i++ )
            {
                // If it can't translate the current file...
                if ( !TranslateFile( files[i] ) )
                {
                    // Increase the amount of errors!
                    errCount++;
                    continue;
                }
            }

            // Once we're done, log information
            Console.WriteLine( "\nRecursive folder translation finished!" );
            Console.WriteLine( $"Errors: {errCount}/{fileCount} ({(float)(errCount / fileCount) * 100}%)" );
        }
        else if ( IsValidVMT( inputFile ) ) // Otherwise, if we have a valid VMT file...
        {
            // Translate just this file
            TranslateFile( inputFile );
        }
    }

    /// <summary>
    /// Translates a specified VMT file to a VMAT file.
    /// </summary>
    /// <param name="path">The path to the VMT file we wish to translate.</param>
    private static bool TranslateFile( string path )
    {
        // Variables
        List<Variable> variables = new List<Variable>(); // List of VMAT translated variables
        List<string> texturePaths = new List<string>(); // List of VTF files this VMT references

        // Log general information
        Console.WriteLine( $"\nFile to translate: \"{path}\"" );
        Console.WriteLine( $"Output file: \"{path}\"" );
        Console.WriteLine( $"Source 2 version: {version}" );
        Console.WriteLine( $"File extension: \".{fileExtension}\"\n" );
        Console.WriteLine( "Translating VMT to VMAT...\n" );

        // Set the VMAT path, if it isn't already specified...
        if ( string.IsNullOrEmpty( outputFile ) || string.IsNullOrEmpty( outputFolder ) )
        {
            // Copy the VMT's path and filename, but change the extension to .vmat
            outputFile = Path.ChangeExtension( path, ".vmat" );
        }
        else if ( !string.IsNullOrEmpty( outputFolder ) ) // Otherwise, if we've specified a folder...
        {
            // Make sure the folder exists
            Directory.CreateDirectory( Path.Combine( outputFolder, Path.GetDirectoryName( path ) ?? string.Empty ) );

            // Set the output file's path
            outputFile = Path.Combine( outputFolder, Path.ChangeExtension( Path.GetFileName( path ), ".vmat" ) );
        }

        // Create a file at the path of the VMAT and start writing to it
        using ( StreamWriter sw = new StreamWriter( File.Create( outputFile ) ) )
        {
            // All of the lines in the VMT file
            string[] lines = File.ReadAllLines( path );

            // Write some information beforehand
            sw.WriteLine( "//" );
            sw.WriteLine( "// THIS FILE WAS AUTOMATICALLY TRANSLATED THROUGH VMT2VMAT" );
            sw.WriteLine( "// IF THERE ARE ANY ISSUES WITH THE PROVIDED TRANSLATION; CONTACT LOKI" );
            sw.WriteLine( "//" );
            sw.WriteLine( $"// INFO: TEXTURE FILE EXTENSION: \".{fileExtension}\", ENGINE VERSION: \"{version}\"" );
            sw.WriteLine( "//" );
            sw.WriteLine( "" );
            sw.WriteLine( "Layer0" );
            sw.WriteLine( "{" );

            // Check every line in the VMT file...
            for ( int i = 0; i < lines.Length; i++ )
            {
                // The current line
                string line = lines[i].TrimStart( '\t' );

                // The two halves of a keyword, the key and the value
                string[] keyvalues = line.Split( ' ', '\t' );

                // For every value in the keyword...
                for ( int j = 0; j < keyvalues.Length; j++ )
                {
                    keyvalues[j] = keyvalues[j].Replace( "\"", "" ); // Replace the quotes with nothing, we add quotes ourselves
                    keyvalues[j] = keyvalues[j].Replace( "\\", "/" ); // Replace backslashes with forward slashes
                    keyvalues[j] = keyvalues[j].ToLower(); // Make all the text lowercase
                }

                // If we haven't already translated the shader...
                if ( !variables.HasVariable( VariableType.Shader ) )
                {
                    // If we can translate the line as a shader...
                    if ( TranslateShader( keyvalues[0], out string vmatShader ) )
                    {
                        // Add a shader to the list of variables in this VMAT
                        variables.Add( new Variable
                        {
                            Key = "shader",
                            Value = vmatShader,
                            Comment = $"// {line}",
                            VmtKeyValue = keyvalues[0],
                            Type = VariableType.Shader,
                            Group = VariableGroup.Shader
                        } );

                        // Skip to the next line
                        continue;
                    }
                    else // Error happened translating the shader!
                    {
                        // Write the issue and return
                        sw.WriteLine( "// FAULT! SHADER FAILED TO TRANSLATE" );
                        return false;
                    }
                }

                // If we can translate the current keyword...
                if ( TranslateKeyValue( keyvalues[0], out string key, out KeyValueType keyType, out VariableType varType, out VariableGroup varGroup ) )
                {
                    // Check against different types of values we have

                    // Depending on the type of value we have...
                    switch ( keyType )
                    {
                        // If it's unknown...
                        case KeyValueType.Unknown:
                            variables.Add( new Variable
                            {
                                Key = string.Empty,
                                Value = string.Empty,
                                Comment = $"// UNKNOWN! {line}",
                                VmtKeyValue = line,
                                Type = varType,
                                Group = varGroup
                            } );
                            break;

                        // If it's a texture...
                        // We should prefix the path with "materials/", as well as add the specified file extension for the textures
                        case KeyValueType.Texture:
                            variables.Add( new Variable
                            {
                                Key = key,
                                Value = $"materials/{keyvalues[1]}.{fileExtension}",
                                Comment = $"// {line}",
                                VmtKeyValue = $"{keyvalues[0]} {keyvalues[1]}",
                                Type = varType,
                                Group = varGroup
                            } );

                            // Add the texture to the list of VTF files to be converted
                            texturePaths.Add( $"materials/{keyvalues[1]}.vtf" );
                            break;

                        // If it's a number or string...
                        case KeyValueType.String:
                        case KeyValueType.Number:
                            variables.Add( new Variable
                            {
                                Key = key,
                                Value = $"{keyvalues[1]}",
                                Comment = $"// {line}",
                                VmtKeyValue = $"{keyvalues[0]} {keyvalues[1]}",
                                Type = varType,
                                Group = varGroup
                            } );
                            break;

                        // If it's a Vector2...
                        // It's effectively an array of floats, parse it as such
                        case KeyValueType.Vector2:
                            variables.Add( new Variable
                            {
                                Key = key,
                                Value = $"[{keyvalues[1]} {keyvalues[2]}]",
                                Comment = $"// {line}",
                                VmtKeyValue = $"{keyvalues[0]} [{keyvalues[1]} {keyvalues[2]}]",
                                Type = VariableType.Vector2,
                                Group = varGroup
                            } );
                            break;

                        // If it's a Vector2 with both values being the same...
                        case KeyValueType.SameValueV2:
                            variables.Add( new Variable
                            {
                                Key = key,
                                Value = $"[{keyvalues[1]} {keyvalues[1]}]",
                                Comment = $"// {line}",
                                VmtKeyValue = $"{keyvalues[0]} [{keyvalues[1]} {keyvalues[1]}]",
                                Type = VariableType.Vector2,
                                Group = varGroup
                            } );
                            break;

                        // If it's a Vector3...
                        // It's effectively an array of floats, parse it as such
                        case KeyValueType.Vector3:
                            variables.Add( new Variable
                            {
                                Key = key,
                                Value = $"[{keyvalues[1]} {keyvalues[2]} {keyvalues[3]}]",
                                Comment = $"// {line}",
                                VmtKeyValue = $"{keyvalues[0]} [{keyvalues[1]} {keyvalues[2]} {keyvalues[3]}]",
                                Type = VariableType.Vector3,
                                Group = varGroup
                            } );
                            break;
                    }
                }
                else // Otherwise!
                {
                    // Skip to the next line
                    continue;
                }
            }

            // If we have a translucency solver, but no translucency texture...
            if ( ( variables.HasVariable( VariableType.Alpha )
                || variables.HasVariable( VariableType.Translucency ) )
                && !variables.HasVariable( VariableType.AlphaTexture ) )
            {
                variables.Add( new Variable
                {
                    Key = "TextureAlpha",
                    Value = variables.GetVariable( "TextureColor" )!.Value.Replace( $".{fileExtension}", $"_trans.{fileExtension}" ),
                    Comment = "// AUTOGENERATED FROM COLOR TEXTURE",
                    Type = VariableType.AlphaTexture,
                    Group = VariableGroup.Alpha
                } );
            }

            // If we have a normal map or roughness texture, but no PBR enabled...
            if ( ( variables.HasVariable( VariableType.NormalTexture )
                || variables.HasVariable( VariableType.RoughnessTexture ) )
                && !variables.HasVariable( VariableType.Specular ) )
            {
                variables.Add( new Variable
                {
                    Key = "F_SPECULAR",
                    Value = "1",
                    Comment = "// PBR ENABLED",
                    VmtKeyValue = "AUTOGENERATED",
                    Type = VariableType.Specular,
                    Group = VariableGroup.Roughness
                } );
            }

            // If we have a self-illum texture, but self-illum is not enabled...
            if ( variables.HasVariable( VariableType.SelfIllumTexture ) &&
                !variables.HasVariable( VariableType.SelfIllum ) )
            {
                variables.Add( new Variable
                {
                    Key = "F_SELF_ILLUM",
                    Value = "1",
                    Comment = "// SELF-ILLUM ENABLED",
                    VmtKeyValue = "AUTOGENERATED",
                    Type = VariableType.SelfIllum,
                    Group = VariableGroup.SelfIllum
                } );
            }
            else if ( variables.HasVariable( VariableType.SelfIllum ) && // BUT, if we have self-illum enabled, but no self-illum texture...
                !variables.HasVariable( VariableType.SelfIllumTexture ) )
            {
                variables.Add( new Variable
                {
                    Key = "TextureSelfIllum",
                    Value = variables.GetVariable( "TextureColor" )!.Value.Replace( $".{fileExtension}", $"_selfillum.{fileExtension}" ),
                    Comment = "// AUTOGENERATED FROM COLOR TEXTURE",
                    VmtKeyValue = "AUTOGENERATED",
                    Type = VariableType.SelfIllumTexture,
                    Group = VariableGroup.SelfIllum
                } );
            }

            Dictionary<VariableGroup, List<Variable>> groups = new();

            // For every variable...
            for ( int i = 0; i < variables.Count; i++ )
            {
                // Get the current variable
                Variable variable = variables[i];

                // If we don't already have this group...
                if ( !groups.ContainsKey( variable.Group ) )
                {
                    // Add it and a new list of variables, including this one
                    groups.Add( variable.Group, new List<Variable>( [variable] ) );
                }
                else // Otherwise...
                {
                    // Get the group and add this variable to its list of variables
                    groups[variable.Group].Add( variable );
                }

                // Order the groups...
                if ( groups.Count > 1 )
                {
                    // Sort the groups by their group type
                    groups = groups.OrderBy( x => x.Key ).ToDictionary( x => x.Key, x => x.Value );
                }
            }

            // For every group...
            foreach ( VariableGroup group in groups.Keys )
            {
                // Write some prefix information
                sw.WriteLine( $"\t// -- {group} --" );

                // For every variable in the group...
                for ( int i = 0; i < groups[group].Count; i++ )
                {
                    // The current variable in the current group
                    Variable variable = groups[group][i];

                    // Translate different variable types to their VMAT equivalent
                    switch ( variable.Type )
                    {
                        default:
                            break;

                        case VariableType.SurfaceProperty:
                            // This is hacky...
                            sw.WriteLine( "\n\tSystemAttributes" );
                            sw.WriteLine( "\t{" );

                            sw.WriteLine( $"\t\tPhysicsSurfaceProperties \"{TranslateSurfaceProperty( variable.Value )}\"" );

                            sw.WriteLine( "\t}\n" );
                            continue;

                        case VariableType.Cubemap:
                            variable.Value = TranslateCubemap( variable.Value );
                            break;

                        case VariableType.Detail:
                            variable.Value = TranslateDetailMode( variable.Value );
                            break;
                    }

                    // Write its information to the VMAT file!
                    sw.WriteLine( $"\t{variable.Key} {( !string.IsNullOrEmpty( variable.Value ) ? $"\"{variable.Value}\"" : "" )} {variable.Comment}" );

                    // Log that we've written the current variable
                    Console.WriteLine( $"\"{variable.VmtKeyValue}\" -> \"{variable.Key} {variable.Value}\"" );
                }

                // Write a new line, for formatting
                sw.WriteLine( "" );
            }

            // Closing remarks
            sw.WriteLine( "}" );
            sw.Close();
        }

        // Log that we're now converting VTFs
        Console.WriteLine( $"\nConverting VTFs to {fileExtension.ToUpper()}s...\n" );

        // Convert our VTFs using VTFEdit to the preferred file extension
        for ( int i = 0; i < texturePaths.Count; i++ )
        {
            // Get the directory of the VMT file
            string vmtDir = Path.GetDirectoryName( path )?.Replace( '\\', '/' ) ?? string.Empty;

            // Get the index of "materials"
            int materialsIndex = vmtDir.IndexOf( "materials" );

            // Remove "materials" and everything after it
            if ( materialsIndex >= 0 )
            {
                vmtDir = vmtDir.Substring( 0, materialsIndex );
            }

            // The current VTF file
            string vtfPath = Path.Combine( vmtDir!, texturePaths[i] );

            // If there's no file at this path...
            if ( !File.Exists( vtfPath ) )
            {
                // Error!
                Console.Error.WriteLine( $"Couldn't find VTF file at path \"{vtfPath}\"!" );
                return false;
            }

            // Create an instance of VTFCMD to convert the VTF to our desired file extension
            Process? vtfCmd = Process.Start( new ProcessStartInfo()
            {
                FileName = "VTFCmd.exe",
                Arguments = $"-file \"{vtfPath}\" -exportformat \"{fileExtension}\" -silent"
            } );

            // Log our conversion!
            Console.WriteLine( $"\"{Path.GetFileName( vtfPath )}\" -> \"{Path.GetFileName( Path.ChangeExtension( vtfPath, fileExtension ) )}\"" );
        }

        // Log our success!
        Console.WriteLine( "\nSuccessfully translated VMT to VMAT!" );
        return true;
    }

    /// <summary>
    /// Checks if the provided path to a VMT is valid.
    /// </summary>
    /// <param name="vmtPath">The path to the VMT file we wish to check.</param>
    /// <returns><see langword="true"/> if the path is valid.</returns>
    private static bool IsValidVMT( string vmtPath )
    {
        // Make sure the path has some sort of content, that the file actually exists, and that it's extension is ".vmt"
        return !string.IsNullOrEmpty( vmtPath ) && File.Exists( vmtPath ) && Path.GetExtension( vmtPath ) == ".vmt";
    }

    /// <summary>
    /// Translates a VMT shader to VMAT.
    /// </summary>
    /// <param name="vmtShader">The VMT shader we wish to translate.</param>
    /// <param name="vmatShader">The VMAT equivalent of the argument shader.</param>
    /// <returns>The VMAT equivalent of the VMT shader.</returns>
    private static bool TranslateShader( string vmtShader, out string vmatShader )
    {
        switch ( vmtShader.ToLower() )
        {
            // No valid shader!
            default:
                Console.Error.WriteLine( "Invalid shader provided!" );
                vmatShader = "Invalid";
                return false;

            // Unlit shader
            case "unlitgeneric":

            // Default shader, used for like 99% of materials
            case "vertexlitgeneric":

            // LightmappedGeneric, used for brushes / static objects
            case "lightmappedgeneric":
                switch ( version )
                {
                    // In HL:A it's known as "VR Complex"
                    default:
                    case EngineVersion.HLA:
                        vmatShader = "vr_complex.vfx";
                        break;

                    // Last I checked, in CS2 it's known as "Complex"
                    case EngineVersion.CS2:
                        vmatShader = "complex.vfx";
                        break;

                    // In s&box it's also known as "Complex", but with extra information
                    case EngineVersion.Sandbox:
                        vmatShader = "shaders/complex.shader";
                        break;
                }
                return true;

            // Per-pixel 2-way blend
            case "worldvertextransition":
                switch ( version )
                {
                    // In HL:A it's known as "VR Simple 2way Blend"
                    default:
                    case EngineVersion.HLA:
                        vmatShader = "vr_simple_2way_blend.vfx";
                        break;
                }
                return true;
        }
    }

    /// <summary>
    /// Tries to translate a VMT keyword to a VMAT one.
    /// </summary>
    /// <param name="vmtKey">The keyword in VMT format.</param>
    /// <param name="vmatKey">The resulting VMAT formatted keyword.</param>
    /// <param name="valType">The type of the value we get.</param>
    /// <returns><see langword="true"/> if we have a successful keyword translation, as well as <paramref name="vmatKey"/> getting a value.</returns>
    private static bool TranslateKeyValue( string vmtKey, out string vmatKey, out KeyValueType valType, out VariableType varType, out VariableGroup varGroup )
    {
        // Ignore empty lines, comments, and other invalid characters
        if ( string.IsNullOrEmpty( vmtKey )
            || vmtKey == "{" || vmtKey == "}"
            || vmtKey.Contains( "//" ) )
        {
            vmatKey = "Unknown";
            valType = KeyValueType.Unknown;
            varType = VariableType.Unknown;
            varGroup = VariableGroup.Unknown;
            return false;
        }

        switch ( vmtKey )
        {
            // If the key isn't recognized
            default:
                Console.Error.WriteLine( $"Unknown VMT key encountered: \"{vmtKey}\"" );
                vmatKey = "Unknown";
                valType = KeyValueType.Unknown;
                varType = VariableType.Unknown;
                varGroup = VariableGroup.Unknown;
                return false;

            // Color texture
            case "$basetexture":
                vmatKey = "TextureColor";
                valType = KeyValueType.Texture;
                varType = VariableType.ColorTexture;
                varGroup = VariableGroup.Color;
                return true;

            // Normal map
            case "$bumpmap":
                vmatKey = "TextureNormal";
                valType = KeyValueType.Texture;
                varType = VariableType.NormalTexture;
                varGroup = VariableGroup.Normal;
                return true;

            // Roughness texture
            case "$phongexponenttexture":
                vmatKey = "TextureRoughness";
                valType = KeyValueType.Texture;
                varType = VariableType.RoughnessTexture;
                varGroup = VariableGroup.Roughness;
                return true;

            // Roughness scale
            case "$phongboost":
                vmatKey = "g_flRoughnessScaleFactor";
                valType = KeyValueType.Number;
                varType = VariableType.Number;
                varGroup = VariableGroup.Roughness;
                return true;

            // Ambient occlusion texture
            case "$ambientocclusiontexture":
                vmatKey = "TextureAmbientOcclusion";
                valType = KeyValueType.Texture;
                varType = VariableType.AOTexture;
                varGroup = VariableGroup.AmbientOcclusion;
                return true;

            // Surface properties
            case "$surfaceprop":
                vmatKey = "PhysicsSurfaceProperties";
                valType = KeyValueType.String;
                varType = VariableType.SurfaceProperty;
                varGroup = VariableGroup.Physics;
                return true;

            // Alpha texture
            case "$translucent":
                vmatKey = "F_ALPHA_TEST";
                valType = KeyValueType.Number;
                varType = VariableType.Alpha;
                varGroup = VariableGroup.Alpha;
                return true;

            // Detail texture
            case "$detail":
                vmatKey = "TextureDetail";
                valType = KeyValueType.Texture;
                varType = VariableType.DetailTexture;
                varGroup = VariableGroup.Detail;
                return true;

            // Scale of the detail texture
            case "$detailscale":
                vmatKey = "g_vDetailTexCoordScale";
                valType = KeyValueType.SameValueV2;
                varType = VariableType.Vector2;
                varGroup = VariableGroup.Detail;
                return true;

            // Blend factor of the detail texture
            case "$detailblendfactor":
                vmatKey = "g_flDetailBlendFactor";
                valType = KeyValueType.Number;
                varType = VariableType.Number;
                varGroup = VariableGroup.Detail;
                return true;

            // Detail blend mode
            case "$detailblendmode":
                vmatKey = "F_DETAIL_TEXTURE";
                valType = KeyValueType.Number;
                varType = VariableType.Detail;
                varGroup = VariableGroup.Detail;
                return true;

            // Cubemap
            case "$envmap":
                vmatKey = "F_SPECULAR_CUBE_MAP";
                valType = KeyValueType.String;
                varType = VariableType.Cubemap;
                varGroup = VariableGroup.Roughness;
                return true;

            // Render backfaces
            case "$nocull":
                vmatKey = "F_RENDER_BACKFACES";
                valType = KeyValueType.Number;
                varType = VariableType.Number;
                varGroup = VariableGroup.Unknown;
                return true;

            // Self-illum
            case "$selfillum":
                vmatKey = "F_SELF_ILLUM";
                valType = KeyValueType.Number;
                varType = VariableType.SelfIllum;
                varGroup = VariableGroup.SelfIllum;
                return true;
        }
    }

    /// <summary>
    /// Directly translates a VMT surface property to an equivalent VMAT one.
    /// </summary>
    /// <param name="vmtSurfProp">The surface property from the VMT.</param>
    /// <returns>The VMAT equivalent of the provided VMT surface property.</returns>
    private static string TranslateSurfaceProperty( string vmtSurfProp )
    {
        switch ( vmtSurfProp )
        {
            default:
                return "Unknown";

            case "concrete":
                switch ( version )
                {
                    default:
                    case EngineVersion.HLA:
                    case EngineVersion.CS2:
                        return "prop.concrete";

                    case EngineVersion.Sandbox:
                        return "concrete";
                }

            case "metal":
                switch ( version )
                {
                    default:
                    case EngineVersion.HLA:
                    case EngineVersion.CS2:
                        return "prop.metal";

                    case EngineVersion.Sandbox:
                        return "metal";
                }
        }
    }

    /// <summary>
    /// Translates a VMT detail mode to a VMAT equivalent.
    /// </summary>
    /// <param name="vmtDetailMode">The VMT detail mode.</param>
    /// <returns>The equivalent VMAT detail blend mode.</returns>
    private static string TranslateDetailMode( string vmtDetailMode )
    {
        switch ( vmtDetailMode )
        {
            default:
                return "Invalid"; // Invalid / unknown detail mode

            case "0": // DecalModulate
                return "1"; // Mod2X
        }
    }

    /// <summary>
    /// Translates a VMT cubemap mode to a corresponding VMAT one.
    /// </summary>
    /// <param name="vmtCubemapMode">The mode the VMT cubemap's using.</param>
    /// <returns>The VMAT equivalent of the VMT cubemap mode.</returns>
    private static string TranslateCubemap( string vmtCubemapMode )
    {
        switch ( vmtCubemapMode )
        {
            default:
                return "Invalid"; // Invalid / unknown cubemap mode

            case "env_cubemap": // Environment / in-game cubemap
                return "1";
        }
    }

    /// <summary>
    /// Checks if an input is a valid argument.
    /// </summary>
    /// <param name="input">The input argument.</param>
    /// <param name="validArg">The valid argument we wish to check against.</param>
    /// <returns><see langword="true"/> if the input is a valid argument.</returns>
    private static bool IsValidArg( string input, string validArg )
    {
        return input.Equals( validArg, StringComparison.OrdinalIgnoreCase );
    }

    /// <summary>
    /// The different versions of Source 2 to handle.<br/>
    /// Each of them have slight differences in e.g. shaders, this lets us easily switch between them.
    /// </summary>
    private enum EngineVersion
    {
        /// <summary>
        /// Half-Life: Alyx.
        /// </summary>
        HLA,

        /// <summary>
        /// Counter-Strike 2.
        /// </summary>
        CS2,

        /// <summary>
        /// s&box.
        /// </summary>
        Sandbox
    }

    /// <summary>
    /// The different type of values a keyword can hold
    /// </summary>
    private enum KeyValueType
    {
        /// <summary>
        /// An unknown keyvalue.
        /// </summary>
        Unknown,

        /// <summary>
        /// A texture keyvalue.
        /// </summary>
        Texture,

        /// <summary>
        /// A keyvalue that's purely text.
        /// </summary>
        String,

        /// <summary>
        /// A keyvalue that's any sort of number, float or integer.
        /// </summary>
        Number,

        /// <summary>
        /// A keyvalue with 2 numbers in one array.
        /// </summary>
        Vector2,

        /// <summary>
        /// A keyvalue with 2 numbers in one array, of which both are the same.
        /// </summary>
        SameValueV2,

        /// <summary>
        /// A keyvalue with 3 numbers in one array.
        /// </summary>
        Vector3,

        /// <summary>
        /// A keyvalue with 3 numbers in one array, of which all are the same.
        /// </summary>
        SameValueV3,

        /// <summary>
        /// A keyvalue with 4 numbers in one array.
        /// </summary>
        Vector4,

        /// <summary>
        /// A keyvalue with 4 numbers in one array, of which all are the same.
        /// </summary>
        SameValueV4
    }
}