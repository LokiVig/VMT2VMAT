﻿namespace VMT2VMAT;

public class Program
{
    /// <summary>
    /// Which version of Source 2 we should parse to.
    /// </summary>
    private static Source2Version version = Source2Version.HLA;

    /// <summary>
    /// Which file type we should use for textures.
    /// </summary>
    private static string fileExtension = "tga";

    /// <summary>
    /// The path to the VMT file we want to translate.
    /// </summary>
    private static string vmtPath = "C:\\Users\\Loke.Vigstrand\\source\\repos\\VMT2VMAT\\build\\Debug\\net9.0\\example\\dog_sheet.vmt";

    /// <summary>
    /// The path to the VMAT file we want to translate.
    /// </summary>
    private static string vmatPath = string.Empty;

    /// <summary>
    /// The list of textures in this material.
    /// </summary>
    private static List<VMATVariable> vmatVariables = new List<VMATVariable>();

    public static void Main( string[] args )
    {
        // Check every argument
        for ( int i = 0; i < args.Length; i++ )
        {
            // If we're passing the path to a VMT file...
            if ( IsValidArg( args[i], "-vmt" ) )
            {
                // Use it!
                vmtPath = args[i + 1];
            }

            // If we're passing the path for the output VMAT...
            if ( IsValidArg( args[i], "-vmat" ) )
            {
                // Use it!
                vmatPath = args[i + 1];
            }

            // If we're specifying a version...
            if ( IsValidArg( args[i], "-version" ) )
            {
                if ( IsValidArg( args[i + 1], "hla" ) ) // Half-Life: Alyx
                {
                    version = Source2Version.HLA;
                }
                else if ( IsValidArg( args[i + 1], "cs2" ) ) // Counter Strike 2
                {
                    version = Source2Version.CS2;
                }
                else if ( IsValidArg( args[i + 1], "sbox" ) ) // s&box
                {
                    version = Source2Version.Sandbox;
                }
                else // Assume invalid input! Default to HL:A
                {
                    Console.Error.WriteLine( "Main: Invalid Source 2 version provided! Defaulting to HLA..." );
                    version = Source2Version.HLA;
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
                    Console.Error.WriteLine( "Main: Invalid file extension provided! Defaulting to TGA..." );
                    fileExtension = "tga";
                }
            }
        }

        // If there was no provided VMAT path...
        if ( string.IsNullOrEmpty( vmatPath ) )
        {
            // Copy the VMT's path and filename, but change the extension to .vmat
            vmatPath = Path.ChangeExtension( vmtPath, ".vmat" );
        }

        // Check if the provided path to a VMT is valid...
        if ( !IsValidVMT( vmtPath ) )
        {
            // If not, whoopsie!
            Console.Error.WriteLine( "Main: Invalid VMT file given!" );
            return;
        }

        // Log general information
        Console.WriteLine( $"\nFile to translate: \"{vmtPath}\"" );
        Console.WriteLine( $"Output file: \"{vmatPath}\"" );
        Console.WriteLine( $"Source 2 version: {version}" );
        Console.WriteLine( $"File extension: \".{fileExtension}\"\n" );
        Console.WriteLine( "Translating VMT to VMAT...\n" );

        // Create a file at the path of the VMAT and start writing to it
        using ( StreamWriter sw = new StreamWriter( File.Create( vmatPath ) ) )
        {
            // All of the lines in the VMT file
            string[] lines = File.ReadAllLines( vmtPath );

            // Write some information beforehand
            sw.WriteLine( "//" );
            sw.WriteLine( "// THIS FILE WAS AUTOMATICALLY TRANSLATED THROUGH VMT2VMAT" );
            sw.WriteLine( "// IF THERE ARE ANY ISSUES WITH THE PROVIDED TRANSLATION; CONTACT LOKI" );
            sw.WriteLine( "//" );
            sw.WriteLine( $"// INFO: TEXTURE FILE EXTENSION: \".{fileExtension}\", VERSION: \"{version}\"" );
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
                if ( !vmatVariables.HasVariable( VMATVariableType.Shader ) )
                {
                    // If we can translate the line as a shader...
                    if ( TranslateShader( keyvalues[0], out string vmatShader ) )
                    {
                        // Add a shader to the list of variables in this VMAT
                        vmatVariables.Add( new VMATVariable
                        {
                            key = "shader",
                            value = vmatShader,
                            comment = $"// {line}",
                            vmtKeyValue = keyvalues[0],
                            type = VMATVariableType.Shader
                        } );

                        // Skip to the next line
                        continue;
                    }
                    else // Error happened translating the shader!
                    {
                        // Write the issue and return
                        sw.WriteLine( "// FAULT! SHADER FAILED TO TRANSLATE" );
                        break;
                    }
                }

                // If we can translate the current keyword...
                if ( TranslateKeyValue( keyvalues[0], out string key, out KeyValueType keyType, out VMATVariableType varType ) )
                {
                    // Check against different types of values we have

                    // Depending on the type of value we have...
                    switch ( keyType )
                    {
                        // If it's unknown...
                        case KeyValueType.Unknown:
                            vmatVariables.Add( new VMATVariable
                            {
                                key = string.Empty,
                                value = string.Empty,
                                comment = $"// UNKNOWN! {line}",
                                vmtKeyValue = line,
                                type = varType
                            } );
                            break;

                        // If it's a texture...
                        // We should prefix the path with "materials/", as well as add the specified file extension for the textures
                        case KeyValueType.Texture:
                            vmatVariables.Add( new VMATVariable
                            {
                                key = key,
                                value = $"materials/{keyvalues[1]}.{fileExtension}",
                                comment = $"// {line}",
                                vmtKeyValue = $"{keyvalues[0]} {keyvalues[1]}",
                                type = varType
                            } );
                            break;

                        // If it's a number or string...
                        case KeyValueType.String:
                        case KeyValueType.Number:
                            vmatVariables.Add( new VMATVariable
                            {
                                key = key,
                                value = $"{keyvalues[1]}",
                                comment = $"// {line}",
                                vmtKeyValue = $"{keyvalues[0]} {keyvalues[1]}",
                                type = varType
                            } );
                            break;

                        // If it's a Vector2...
                        // It's effectively an array of floats, parse it as such
                        case KeyValueType.Vector2:
                            vmatVariables.Add( new VMATVariable
                            {
                                key = key,
                                value = $"[{keyvalues[1]} {keyvalues[2]}]",
                                vmtKeyValue = $"{keyvalues[0]} [{keyvalues[1]} {keyvalues[2]}]",
                                type = VMATVariableType.Vector2
                            } );
                            break;

                        // If it's a Vector2 with both values being the same...
                        case KeyValueType.SameValueV2:
                            vmatVariables.Add( new VMATVariable
                            {
                                key = key,
                                value = $"[{keyvalues[1]} {keyvalues[1]}]",
                                vmtKeyValue = $"{keyvalues[0]} [{keyvalues[1]} {keyvalues[1]}]",
                                type = VMATVariableType.Vector2
                            } );
                            break;

                        // If it's a Vector3...
                        // It's effectively an array of floats, parse it as such
                        case KeyValueType.Vector3:
                            vmatVariables.Add( new VMATVariable
                            {
                                key = key,
                                value = $"[{keyvalues[1]} {keyvalues[2]} {keyvalues[3]}]",
                                vmtKeyValue = $"{keyvalues[0]} [{keyvalues[1]} {keyvalues[2]} {keyvalues[3]}]",
                                type = VMATVariableType.Vector3
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
            if ( ( vmatVariables.HasVariable( VMATVariableType.Alpha )
                || vmatVariables.HasVariable( VMATVariableType.Translucency ) )
                && !vmatVariables.HasVariable( VMATVariableType.AlphaTexture ) )
            {
                vmatVariables.Add( new VMATVariable
                {
                    key = "TextureAlpha",
                    value = vmatVariables.GetVariable( "TextureColor" ).value.Replace( $".{fileExtension}", $"_trans.{fileExtension}" ),
                    comment = "// AUTOGENERATED FROM COLOR TEXTURE",
                    type = VMATVariableType.AlphaTexture
                } );
            }

            // For every variable...
            for (int i = 0; i < vmatVariables.Count; i++ )
            {
                // Get the current variable
                VMATVariable variable = vmatVariables[i];

                // Translate different variable's types to their VMAT equivalent
                switch ( variable.type )
                {
                    default:
                        break;

                    case VMATVariableType.SurfaceProperty:
                        // This is hacky...
                        sw.WriteLine( "\n\tSystemAttributes" );
                        sw.WriteLine( "\t{" );

                        sw.WriteLine( $"\t\tPhysicsSurfaceProperties \"{TranslateSurfaceProperty( variable.value )}\"" );

                        sw.WriteLine( "\t}\n" );
                        continue;

                    case VMATVariableType.Cubemap:
                        variable.value = TranslateCubemap( variable.value );
                        break;

                    case VMATVariableType.Detail:
                        variable.value = TranslateDetailMode( variable.value );
                        break;
                }

                // Write its information to the VMAT file!
                sw.WriteLine( $"\t{variable.key} {( !string.IsNullOrEmpty( variable.value ) ? $"\"{variable.value}\"" : "" )} {variable.comment}" );

                // Log that we've written the current variable
                Console.WriteLine( $"\"{variable.vmtKeyValue}\" -> \"{variable.key} {variable.value}\"" );
            }

            // Closing remarks
            sw.WriteLine( "}" );
        }

        // Log our success!
        Console.WriteLine( "\nSuccessfully translated VMT to VMAT!" );
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
                Console.Error.WriteLine( "TranslateShader: Invalid shader given!" );
                vmatShader = "Invalid";
                return false;

            // Default shader, used for like 99% of materials
            case "vertexlitgeneric":

            // LightmappedGeneric, used for floors and walls and stuff
            case "lightmappedgeneric":
                switch ( version )
                {
                    // In HL:A it's known as "VR Complex"
                    default:
                    case Source2Version.HLA:
                        vmatShader = "vr_complex.vfx";
                        break;

                    // Last I checked, in CS2 it's known as "Complex"
                    case Source2Version.CS2:
                        vmatShader = "complex.vfx";
                        break;

                    // In s&box it's also known as "Complex", but with extra information
                    case Source2Version.Sandbox:
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
                    case Source2Version.HLA:
                        vmatShader = "vr_simple_2way_blend.vfx";
                        break;
                }
                return true;
        }
    }

    /// <summary>
    /// Tries to translate a VMT keyword to a VMAT one.
    /// </summary>
    /// <param name="vmtKeyword">The keyword in VMT format.</param>
    /// <param name="vmatKeyword">The resulting VMAT formatted keyword.</param>
    /// <param name="valueType">The type of the value we get.</param>
    /// <returns><see langword="true"/> if we have a successful keyword translation, as well as <paramref name="vmatKeyword"/> getting a value.</returns>
    private static bool TranslateKeyValue( string vmtKeyword, out string vmatKeyword, out KeyValueType valueType, out VMATVariableType varType )
    {
        if ( string.IsNullOrEmpty( vmtKeyword )
            || vmtKeyword == "{" || vmtKeyword == "}"
            || vmtKeyword == "//" || vmtKeyword.Contains( "//" ) )
        {
            vmatKeyword = "Unknown";
            valueType = KeyValueType.Unknown;
            varType = VMATVariableType.Unknown;
            return false;
        }

        switch ( vmtKeyword )
        {
            // Color texture
            case "$basetexture":
                vmatKeyword = "TextureColor";
                valueType = KeyValueType.Texture;
                varType = VMATVariableType.ColorTexture;
                return true;

            // Normal map
            case "$bumpmap":
                vmatKeyword = "TextureNormal";
                valueType = KeyValueType.Texture;
                varType = VMATVariableType.NormalTexture;
                return true;

            // Roughness texture
            case "$phongexponent":
            case "$phongexponenttexture":
                vmatKeyword = "TextureRoughness";
                valueType = KeyValueType.Texture;
                varType = VMATVariableType.RoughnessTexture;
                return true;

            // Unknown for now... Might be something scalar with the intensity of the roughness
            // texture, no clue
            case "$phongboost":
                vmatKeyword = "Unknown";
                valueType = KeyValueType.Unknown;
                varType = VMATVariableType.Unknown;
                return true;

            // Surface properties
            case "$surfaceprop":
                vmatKeyword = "PhysicsSurfaceProperties";
                valueType = KeyValueType.String;
                varType = VMATVariableType.SurfaceProperty;
                return true;

            // Alpha texture
            case "$translucent":
                vmatKeyword = "F_ALPHA_TEST";
                valueType = KeyValueType.Number;
                varType = VMATVariableType.Alpha;
                return true;

            // Detail texture
            case "$detail":
                vmatKeyword = "TextureDetail";
                valueType = KeyValueType.Texture;
                varType = VMATVariableType.DetailTexture;
                return true;

            // Scale of the detail texture
            case "$detailscale":
                vmatKeyword = "g_vDetailTexCoordScale";
                valueType = KeyValueType.SameValueV2;
                varType = VMATVariableType.Vector2;
                return true;

            // Blend factor of the detail texture
            case "$detailblendfactor":
                vmatKeyword = "g_flDetailBlendFactor";
                valueType = KeyValueType.Number;
                varType = VMATVariableType.Number;
                return true;

            // Detail blend mode
            case "$detailblendmode":
                vmatKeyword = "F_DETAIL_TEXTURE";
                valueType = KeyValueType.Number;
                varType = VMATVariableType.Detail;
                return true;

            // Cubemap
            case "$envmap":
                vmatKeyword = "F_SPECULAR_CUBE_MAP";
                valueType = KeyValueType.String;
                varType = VMATVariableType.Cubemap;
                return true;
        }

        Console.Error.WriteLine( $"Unknown keyword encountered: \"{vmtKeyword}\"" );
        vmatKeyword = "Unknown";
        valueType = KeyValueType.Unknown;
        varType = VMATVariableType.Unknown;
        return false;
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

            case "metal":
                switch ( version )
                {
                    default:
                    case Source2Version.HLA:
                    case Source2Version.CS2:
                        return "prop.metal";

                    case Source2Version.Sandbox:
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
                return "Invalid"; // Invalid / unknown detail mode

            case "env_cubemap": // Environment / in-game cubemap
                return "1";
        }
    }

    /// <summary>
    /// Checks if a user input is a valid argument.
    /// </summary>
    /// <param name="userInput">The user's input while using the program.</param>
    /// <param name="validArg">The valid argument we wish to check against.</param>
    /// <returns><see langword="true"/> if the user's input is a valid argument.</returns>
    private static bool IsValidArg( string userInput, string validArg )
    {
        return userInput.Equals( validArg, StringComparison.OrdinalIgnoreCase );
    }

    /// <summary>
    /// The different versions of Source 2 to handle.<br/>
    /// Each of them have slight differences in e.g. shaders, this lets us easily switch between them.
    /// </summary>
    private enum Source2Version
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