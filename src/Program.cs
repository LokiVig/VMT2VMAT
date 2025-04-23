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

    //
    // Different value types we should check if we have, to parse correctly.
    // This could be checking if we've already translated e.g. self-illum, detail, etc.
    //

    private static bool hasShader = false; // Determines if we've already translated the shader
    private static bool hasSurfProp = false; // Determines if we've already translated the surface properties
    private static bool hasDetail = false; // Determines if we've already included the basic things to allow for detail textures

    private static bool shouldHaveSurfProp = false; // Determines if we SHOULD translate the surface properties
    private static bool shouldHaveDetail = false; // Determines if we SHOULD have detail functionality

    public static void Main(string[] args)
    {
        // Check every argument
        for (int i = 0; i < args.Length; i++)
        {
            // If we're passing the path to a VMT file...
            if (IsValidArg(args[i], "-vmt"))
            {
                // Use it!
                vmtPath = args[i + 1];
            }

            // If we're passing the path for the output VMAT...
            if (IsValidArg(args[i], "-vmat"))
            {
                // Use it!
                vmatPath = args[i + 1];
            }

            // If we're specifying a version...
            if (IsValidArg(args[i], "-version"))
            {
                if (IsValidArg(args[i + 1], "hla")) // Half-Life: Alyx
                {
                    version = Source2Version.HLA;
                }
                else if (IsValidArg(args[i + 1], "cs2")) // Counter Strike 2
                {
                    version = Source2Version.CS2;
                }
                else if (IsValidArg(args[i + 1], "sbox")) // s&box
                {
                    version = Source2Version.SBox;
                }
                else // Default is HL:A
                {
                    version = Source2Version.HLA;
                }
            }

            // If we're specifying the file extension for our textures...
            if (IsValidArg(args[i], "-textureextension"))
            {
                if (IsValidArg(args[i + 1], "tga")) // TGA
                {
                    textureExtension = "tga";
                }
                else if (IsValidArg(args[i + 1], "png")) // PNG
                {
                    textureExtension = "png";
                }
                else if (IsValidArg(args[i + 1], "jpg") || IsValidArg(args[i + 1], "jpeg")) // JPG
                {
                    textureExtension = "jpg";
                }
            }
        }

        // If there was no provided VMAT path...
        if (string.IsNullOrEmpty(vmatPath))
        {
            // Copy the VMT's path and filename, but change the extension to .vmat
            vmatPath = Path.ChangeExtension(vmtPath, ".vmat");
        }

        // Check if the provided path to a VMT is valid...
        if (!IsValidVMT(vmtPath))
        {
            // If not, whoopsie!
            Console.Error.WriteLine("Main(string[]): Invalid VMT file given!");
            return;
        }

        // Create a file at the path of the VMAT and start writing to it
        using (StreamWriter sw = new StreamWriter(File.Create(vmatPath)))
        {
            // The translated variable
            string translated = string.Empty;

            // Determines if we should translate the shader
            bool hasShader = false;

            // All of the lines in the VMT file
            string[] lines = File.ReadAllLines(vmtPath);

            // Write some information beforehand
            sw.WriteLine("// THIS FILE WAS AUTOMATICALLY TRANSLATED THROUGH VMT2VMAT");
            sw.WriteLine("// IF THERE IS ANY ISSUE; CONTACT LOKI");
            sw.WriteLine("");
            sw.WriteLine("Layer0");
            sw.WriteLine("{");

            // Check every line in the VMT file...
            for (int i = 0; i < lines.Length; i++)
            {
                // The current line
                string line = lines[i].TrimStart('\t');

                // If we haven't already translated the shader...
                if (!hasShader)
                {
                    // If we can translate the line as a shader...
                    if (TranslateShader(line, out translated))
                    {
                        // Write the shader!
                        sw.WriteLine($"\tshader \"{translated}\" // {line}\n");

                        // We now also have a shader
                        hasShader = true;

                        // Skip to the next line
                        continue;
                    }
                    else // Error happened translating the shader!
                    {
                        // Write the issue and return
                        sw.WriteLine("// FAULT! SHADER FAILED TO TRANSLATE");
                        break;
                    }
                }

                // The two halves of a keyword, the key and the value
                string[] keyword = line.Split(" ");

                // For every value in the keyword...
                for (int j = 0; j < keyword.Length; j++)
                {
                    keyword[j] = keyword[j].Replace("\"", ""); // Replace the quotes with nothing, we add quotes ourselves
                    keyword[j] = keyword[j].Replace("\\", "/"); // Replace backslashes with forward slashes
                    keyword[j] = keyword[j].ToLower(); // Make all the text lowercase
                }

                // If we can translate the current keyword...
                if (TranslateKeyword(keyword[0], out translated, out KeywordValueType type))
                {
                    // Translated value
                    // Makes it easier to both log what we've written and actually write what's necessary
                    string translatedValue = string.Empty;

                    // Determines if we should directly write a value (e.g. when we're not specifying a surface property)
                    bool writeValue = true;

                    // If we should have detail, but we don't already...
                    if (shouldHaveDetail && !hasDetail)
                    {
                        // Get the type of detail texture this is
                        sw.WriteLine($"\n\t{translated} {TranslateDetailMode(keyword[1])} // {line}\n");

                        // We now have detail!
                        shouldHaveDetail = false;
                        hasDetail = true;

                        // We shouldn't later write this again
                        writeValue = false;
                    }

                    // Depending on the type of value we have...
                    switch (type)
                    {
                        // If it's a texture...
                        // We should prefix the path with "materials/", as well as add the specified file extension for the textures
                        case KeywordValueType.Texture:
                            translatedValue = $"{translated} \"materials/{keyword[1]}.{textureExtension}\"";
                            break;

                        // If it's a number or string...
                        // Just write the value as is
                        case KeywordValueType.String:
                        case KeywordValueType.Number:
                            translatedValue = $"{translated} \"{keyword[1]}\"";

                            // If we should translate the surface properties, but we haven't already...
                            if (shouldHaveSurfProp && !hasSurfProp)
                            {
                                // Prefix
                                sw.WriteLine("\n\tSystemAttributes");
                                sw.WriteLine("\t{");

                                // Actual surface property
                                sw.WriteLine($"\t\t{translated} \"{TranslateSurfaceProperty(keyword[1])}\" // {line}");

                                // Suffix
                                sw.WriteLine("\t}\n");

                                // We have now translated surface properties!
                                shouldHaveSurfProp = false;
                                hasSurfProp = true;

                                // We shouldn't have to write this again right after
                                writeValue = false;
                            }
                            break;

                        // If it's a Vector2...
                        // It's effectively an array of floats, parse it as such
                        case KeywordValueType.Vector2:
                            translatedValue = $"{translated} \"[{keyword[1]} {keyword[2]}]\"";
                            break;

                        // If it's a Vector2 with both values being the same...
                        case KeywordValueType.SameValueV2:
                            translatedValue = $"{translated} \"[{keyword[1]} {keyword[1]}]\"";
                            break;

                        // If it's a Vector3...
                        // It's effectively an array of floats, parse it as such
                        case KeywordValueType.Vector3:
                            translatedValue = $"{translated} \"[{keyword[1]} {keyword[2]} {keyword[3]}]\"";
                            break;
                    }

                    // If we should directly write our value afterwards...
                    if (writeValue)
                    {
                        // Write to the file and log a successful translation
                        sw.WriteLine($"\t{translatedValue} // {line}");
                        Console.WriteLine($"Translated keyword \"{line}\" to \"{translatedValue}\"");
                    }
                }
                else // Otherwise!
                {
                    // Skip to the next line
                    continue;
                }
            }

            // Closing remarks
            sw.WriteLine("}");
        }
    }

    /// <summary>
    /// Checks if the provided path to a VMT is valid.
    /// </summary>
    /// <param name="vmtPath">The path to the VMT file we wish to check.</param>
    /// <returns><see langword="true"/> if the path is valid.</returns>
    private static bool IsValidVMT(string vmtPath)
    {
        // Make sure the path has some sort of content, that the file actually exists, and that it's extension is ".vmt"
        return !string.IsNullOrEmpty(vmtPath) && File.Exists(vmtPath) && Path.GetExtension(vmtPath) == ".vmt";
    }

    /// <summary>
    /// Translates a VMT shader to VMAT.
    /// </summary>
    /// <param name="vmtShader">The VMT shader we wish to translate.</param>
    /// <param name="vmatShader">The VMAT equivalent of the argument shader.</param>
    /// <returns>The VMAT equivalent of the VMT shader.</returns>
    private static bool TranslateShader(string vmtShader, out string vmatShader)
    {
        switch (vmtShader.ToLower())
        {
            // No valid shader!
            default:
                Console.Error.WriteLine("TranslateShader(string, out string): Invalid shader given!");
                vmatShader = "invalid";
                return false;

            // Default shader, used for like 99% of materials
            case "vertexlitgeneric":

            // LightmappedGeneric, used for floors and walls and stuff
            case "lightmappedgeneric":
                switch (version)
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
                    case Source2Version.SBox:
                        vmatShader = "shaders/complex.shader";
                        break;
                }
                return true;

            // Per-pixel 2-way blend
            case "worldvertextransition":
                switch (version)
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
    private static bool TranslateKeyword(string vmtKeyword, out string vmatKeyword, out KeywordValueType valueType)
    {
        if (string.IsNullOrEmpty(vmtKeyword) || vmtKeyword == "{" || vmtKeyword == "}")
        {
            vmatKeyword = "Unknown";
            valueType = KeywordValueType.Unknown;
            return false;
        }

        switch (vmtKeyword)
        {
            // Color texture
            case "$basetexture":
                vmatKeyword = "TextureColor";
                valueType = KeywordValueType.Texture;
                return true;

            // Normal map
            case "$bumpmap":
                vmatKeyword = "TextureNormal";
                valueType = KeywordValueType.Texture;
                return true;

            // Surface properties
            case "$surfaceprop":
                vmatKeyword = "PhysicsSurfaceProperties";
                valueType = KeywordValueType.String;
                shouldHaveSurfProp = true; // We're now defining the surface properties
                return true;

            // Detail texture
            case "$detail":
                vmatKeyword = "TextureDetail";
                valueType = KeywordValueType.Texture;
                return true;

            // Scale of the detail texture
            case "$detailscale":
                vmatKeyword = "g_vDetailTexCoordScale";
                valueType = KeywordValueType.SameValueV2;
                return true;

            // Blend factor of the detail texture
            case "$detailblendfactor":
                vmatKeyword = "g_flDetailBlendFactor";
                valueType = KeywordValueType.Number;
                return true;

            // Detail blend mode
            case "$detailblendmode":
                vmatKeyword = "F_DETAIL_TEXTURE";
                valueType = KeywordValueType.Number;
                shouldHaveDetail = true; // We should have detail information
                return true;
        }

        Console.WriteLine($"TranslateKeyword(string, out string): Unknown keyword encountered: \"{vmtKeyword}\"");
        vmatKeyword = "Unknown";
        valueType = KeywordValueType.Unknown;
        return false;
    }

    /// <summary>
    /// Directly translates a VMT surface property to an equivalent VMAT one.
    /// </summary>
    /// <param name="vmtSurfProp">The surface property from the VMT.</param>
    /// <returns>The VMAT equivalent of the provided VMT surface property.</returns>
    private static string TranslateSurfaceProperty(string vmtSurfProp)
    {
        switch (vmtSurfProp)
        {
            default:
                return "Unknown";

            case "metal":
                return "prop.metal";
        }
    }

    /// <summary>
    /// Translates a VMT detail mode to a VMAT equivalent.
    /// </summary>
    /// <param name="vmtDetailMode">The VMT detail mode.</param>
    /// <returns>The equivalent VMAT detail blend mode.</returns>
    private static string TranslateDetailMode(string vmtDetailMode)
    {
        switch (vmtDetailMode)
        {
            default:
                return "Invalid"; // Invalid

            case "0": // DecalModulate
                return "3"; // Normals
        }
    }

    /// <summary>
    /// Checks if a user input is a valid argument.
    /// </summary>
    /// <param name="userInput">The user's input while using the program.</param>
    /// <param name="validArg">The valid argument we wish to check against.</param>
    /// <returns><see langword="true"/> if the user's input is a valid argument.</returns>
    private static bool IsValidArg(string userInput, string validArg)
    {
        return userInput.Equals(validArg, StringComparison.OrdinalIgnoreCase);
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
        Number,
        String,
        Vector2,
        SameValueV2,
        Vector3,
        SameValueV3,
        Vector4,
        SameValueV4
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