namespace VMT2VMAT;

public class Program
{
    /// <summary>
    /// Which version of Source 2 we should parse to.
    /// </summary>
    private static Source2Version version = Source2Version.HLA;

    /// <summary>
    /// Which file type we should use for textures.
    /// </summary>
    private static string textureExtension = "tga";

    /// <summary>
    /// The path to the VMT file we want to translate.
    /// </summary>
    private static string vmtPath = string.Empty;

    /// <summary>
    /// The path to the VMAT file we want to translate.
    /// </summary>
    private static string vmatPath = string.Empty;

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
                    version = Source2Version.SBox;
                }
                else // Default is HL:A
                {
                    version = Source2Version.HLA;
                }
            }

            if ( IsValidArg( args[i], "-textureextension" ) )
            {
                if ( IsValidArg( args[i + 1], "tga" ) )
                {
                    textureExtension = "tga";
                }
                else if ( IsValidArg( args[i + 1], "png" ) )
                {
                    textureExtension = "png";
                }
                else if ( IsValidArg( args[i + 1], "jpg" ) || IsValidArg( args[i + 1], "jpeg" ) )
                {
                    textureExtension = "jpg";
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
            Console.Error.WriteLine( "Main(string[]): Invalid VMT file given!" );
            return;
        }

        // Create a file at the path of the VMAT and start writing to it
        using ( StreamWriter sw = new StreamWriter( File.Create( vmatPath ) ) )
        {
            // The translated variable
            string translated = string.Empty;

            // Determines if we should translate the shader
            bool hasShader = false;

            // All of the lines in the VMT file
            string[] lines = File.ReadAllLines( vmtPath );

            // Write some information beforehand
            sw.WriteLine( "// THIS FILE WAS AUTOMATICALLY TRANSLATED THROUGH VMT2VMAT" );
            sw.WriteLine( "// IF THERE IS ANY ISSUE; GO FUCK YOURSELF" );
            sw.WriteLine( "" );
            sw.WriteLine( "Layer0" );
            sw.WriteLine( "{" );

            // Check every line in the VMT file...
            for ( int i = 0; i < lines.Length; i++ )
            {
                // The current line
                string line = lines[i];

                // If we haven't already translated the shader...
                if ( !hasShader )
                {
                    // If we can translate the line as a shader...
                    if ( TranslateShader( line, out translated ) )
                    {
                        // Write the shader!
                        sw.WriteLine( $"\tshader \"{translated}\" // {line}\n" );

                        // We now also have a shader
                        hasShader = true;

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

                // The two halves of a keyword, the key and the value
                string[] keyword = line.Split( " " );

                // If we can translate the current keyword...
                if ( TranslateKeyword( keyword[0], out translated, out KeywordValueType type ) )
                {
                    string translatedValue = string.Empty;

                    switch ( type )
                    {
                        case KeywordValueType.Texture:
                            translatedValue = $"{translated} \"materials/{keyword[1].Replace( "\"", "" )}.{textureExtension}\"";

                            sw.WriteLine( $"\t{translatedValue} // {line.Remove( 0, 1 )}" );
                            Console.WriteLine( $"Translated keyword \"{line.Remove( 0, 1 )}\" to \"{translatedValue}\"" );
                            break;

                        case KeywordValueType.Float:
                            translatedValue = $"{translated} {keyword[1]}";

                            sw.WriteLine( $"\t{translatedValue} // {line.Remove( 0, 1 )}" );
                            Console.WriteLine( $"Translated keyword \"{line.Remove( 0, 1 )}\" to \"{translated} {keyword[1]}\"" );
                            break;

                        case KeywordValueType.Vector3:
                            translatedValue = $"{translated} [{keyword[1]} {keyword[2]} {keyword[3]}";

                            sw.WriteLine( $"\t{translatedValue} // {line.Remove( 0, 1 )}" );
                            Console.WriteLine( $"Translated keyword \"{line.Remove( 0, 1 )}\" to \"{translatedValue}\"" );
                            break;

                        default:
                            // If we don't know what to do with the keyword, just write it as is
                            sw.WriteLine( $"// {line.Remove( 0, 1 )}" );
                            Console.WriteLine( $"Unknown keyword \"{line.Remove( 0, 1 )}\"" );
                            break;
                    }
                }
                else // Otherwise!
                {
                    // Skip to the next line
                    continue;
                }
            }

            // Closing remarks
            sw.WriteLine( "}" );
        }
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
    /// <param name="vmtShader">The shader in question.</param>
    /// <returns>The VMAT equivalent of the VMT shader.</returns>
    private static bool TranslateShader( string vmtShader, out string vmatShader )
    {
        switch ( vmtShader.ToLower() )
        {
            // No valid shader!
            default:
                Console.Error.WriteLine( "TranslateShader(string, out string): Invalid shader given!" );
                vmatShader = "invalid";
                return false;

            // Default shader, used for like 99% of materials
            case "\"vertexlitgeneric\"":
            case "vertexlitgeneric":

            // LightmappedGeneric, used for floors and walls and stuff
            case "\"lightmappedgeneric\"":
            case "lightmappedgeneric":
                switch ( version )
                {
                    // In HL:A it's known as "VR Complex"
                    default:
                    case Source2Version.HLA:
                        vmatShader = "vr_complex";
                        break;

                    // Last I checked, in CS2 it's known as "Complex"
                    case Source2Version.CS2:
                        vmatShader = "complex";
                        break;

                    // In s&box it's also known as "Complex", but with extra information
                    case Source2Version.SBox:
                        vmatShader = "shaders/complex.shader";
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
    /// <returns><see langword="true"/> if we have a successful keyword translation, as well as <paramref name="vmatKeyword"/> getting a value.</returns>
    private static bool TranslateKeyword( string vmtKeyword, out string vmatKeyword, out KeywordValueType valueType )
    {
        if ( vmtKeyword.Contains( "\"$basetexture\"" ) ||
            vmtKeyword.Contains( "$basetexture" ) )
        {
            vmatKeyword = "TextureColor";
            valueType = KeywordValueType.Texture;
            return true;
        }

        if ( vmtKeyword.Contains( "\"$bumpmap\"" ) ||
             vmtKeyword.Contains( "$bumpmap" ) )
        {
            vmatKeyword = "TextureNormal";
            valueType = KeywordValueType.Texture;
            return true;
        }

        Console.WriteLine( $"TranslateKeyword(string, out string): Unknown keyword encountered: \"{vmtKeyword}\"" );
        vmatKeyword = "Unknown";
        valueType = KeywordValueType.Unknown;
        return false;
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
        HLA, // Half-Life: Alyx
        CS2, // Counter Strike 2
        SBox // s&box
    }

    /// <summary>
    /// The different type of values a keyword can hold
    /// </summary>
    private enum KeywordValueType
    {
        Unknown,
        Texture,
        Float,
        Vector3
    }

    /// <summary>
    /// The different file type that is allowed for textures in S2
    /// </summary>
    private enum TextureFileType
    {
        tga,
        png,
        jpg,
        psd
    }
}