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
    /// Determines if folder translation is recursive or not.
    /// </summary>
    private static bool recursive = false;

    /// <summary>
    /// Does everything necessary to actually translate a VMT to a VMAT.
    /// </summary>
    /// <param name="args">The user's input arguments.</param>
    public static void Main(string[] args)
    {
        // Check every argument
        for (int i = 0; i < args.Length; i++)
        {
            // If we're passing the path to a VMT file...
            if (IsValidArg(args[i], "-input")
                || IsValidArg(args[i], "-i"))
            {
                // If the argument features an extension...
                if (Path.HasExtension(args[i + 1]))
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
            if (IsValidArg(args[i], "-output")
                || IsValidArg(args[i], "-o"))
            {
                // If the argument features an extension...
                if (Path.HasExtension(args[i + 1]))
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

            // If we're specifying a recursive folder translation...
            if (IsValidArg(args[i], "-recursive")
                || IsValidArg(args[i], "-r"))
            {
                // Set the recursive variable to true
                recursive = true;
            }

            // If we're specifying a version...
            if (IsValidArg(args[i], "-version")
                || IsValidArg(args[i], "-v"))
            {
                if (IsValidArg(args[i + 1], "hla")) // Half-Life: Alyx
                {
                    version = EngineVersion.HLA;
                }
                else if (IsValidArg(args[i + 1], "cs2")) // Counter Strike 2
                {
                    version = EngineVersion.CS2;
                }
                else if (IsValidArg(args[i + 1], "sbox")) // s&box
                {
                    version = EngineVersion.Sandbox;
                }
                else // Assume invalid input! Default to HL:A
                {
                    Console.Error.WriteLine("Invalid Source 2 version provided! Defaulting to HLA...");
                    version = EngineVersion.HLA;
                }
            }

            // If we're specifying the file extension for our textures...
            if (IsValidArg(args[i], "-fileextension")
                || IsValidArg(args[i], "-fe"))
            {
                if (IsValidArg(args[i + 1], "tga")) // TGA
                {
                    fileExtension = "tga";
                }
                else if (IsValidArg(args[i + 1], "png")) // PNG
                {
                    fileExtension = "png";
                }
                else if (IsValidArg(args[i + 1], "jpg")
                    || IsValidArg(args[i + 1], "jpeg")) // JPG
                {
                    fileExtension = "jpg";
                }
                else // Assume invalid input! Default to TGA
                {
                    Console.Error.WriteLine("Invalid file extension provided! Defaulting to TGA...");
                    fileExtension = "tga";
                }
            }
        }

        // If we're passing a VMT file and a folder...
        if (!string.IsNullOrEmpty(inputFile) && !string.IsNullOrEmpty(inputFolder))
        {
            // Tell the user it's not possible!
            Console.Error.WriteLine("Can't specify a VMT path AND folder!");
            return;
        }

        // Check if the provided path to a VMT is valid...
        if (!IsValidVMT(inputFile) && !Directory.Exists(inputFolder))
        {
            // If not, whoopsie!
            Console.Error.WriteLine("Invalid VMT file / folder given!");
            return;
        }

        // If we have a valid VMT file...
        if (IsValidVMT(inputFile))
        {
            // Translate just this file
            TranslateFile(inputFile);
        }
        else if (Directory.Exists(inputFolder)) // Otherwise, if the input folder is a valid directory...
        {
            string[] files = Directory.GetFiles(inputFolder, "*.vmt", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            int fileCount = files.Length; // The total amount of files
            int errCount = 0; // The total amount of errors

            // Start a new stopwatch, we need to record just how long it takes to do this!
            Stopwatch stopwatch = Stopwatch.StartNew();

            // For every file in this folder and all recursive folders...
            Parallel.ForEach(files, file =>
            {
                // If it can't translate the current file...
                if (!TranslateFile(file))
                {
                    // Increase the amount of errors!
                    Interlocked.Increment(ref errCount);
                }
            });

            // Stop the stopwatch! We've done it all
            stopwatch.Stop();

            // Once we're done, log information
            Console.WriteLine("\nRecursive folder translation finished!");
            Console.WriteLine($"{errCount} errors / {fileCount} files ({(float)errCount / fileCount:P2})");
            Console.WriteLine(@$"Time to complete: {stopwatch.Elapsed:m\:ss\.fff}");
        }
    }

    /// <summary>
    /// Translates a specified VMT file to a VMAT file.
    /// </summary>
    /// <param name="path">The path to the VMT file we wish to translate.</param>
    private static bool TranslateFile(string path)
    {
        // Variables
        List<Variable> variables = new List<Variable>(); // List of VMAT translated variables
        List<string> texturePaths = new List<string>(); // List of VTF files this VMT references

        // Log general information
        Console.WriteLine($"\nFile to translate: \"{path}\"");
        Console.WriteLine($"Output file: \"{path}\"");
        Console.WriteLine($"Source 2 version: {version}");
        Console.WriteLine($"File extension: \".{fileExtension}\"\n");
        Console.WriteLine("Translating VMT to VMAT...\n");

        // Set the VMAT path, if it isn't already specified...
        if (string.IsNullOrEmpty(outputFile) || string.IsNullOrEmpty(outputFolder))
        {
            // Copy the VMT's path and filename, but change the extension to .vmat
            outputFile = Path.ChangeExtension(path, ".vmat");
        }
        else if (!string.IsNullOrEmpty(outputFolder)) // Otherwise, if we've specified a folder...
        {
            // Make sure the folder exists
            Directory.CreateDirectory(Path.Combine(outputFolder, Path.GetDirectoryName(path) ?? string.Empty));

            // Set the output file's path
            outputFile = Path.Combine(outputFolder, Path.ChangeExtension(Path.GetFileName(path), ".vmat"));
        }

        // Create a file at the path of the VMAT and start writing to it
        using (StreamWriter sw = new StreamWriter(File.Create(outputFile)))
        {
            // All of the lines in the VMT file
            string[] lines = File.ReadAllLines(path);

            // Write some information beforehand
            sw.WriteLine("//");
            sw.WriteLine("// THIS FILE WAS AUTOMATICALLY TRANSLATED THROUGH VMT2VMAT V.1.2");
            sw.WriteLine("// IF THERE ARE ANY ISSUES WITH THE PROVIDED TRANSLATION; CONTACT LOKI");
            sw.WriteLine("//");
            sw.WriteLine($"// INFO: TEXTURE FILE EXTENSION: \".{fileExtension}\", ENGINE VERSION: \"{version}\"");
            sw.WriteLine("//");
            sw.WriteLine("");
            sw.WriteLine("Layer0");
            sw.WriteLine("{");

            // Check every line in the VMT file...
            for (int i = 0; i < lines.Length; i++)
            {
                // The current line
                string line = lines[i].TrimStart('\t');

                // The two halves of a keyword, the key and the value
                string[] keyvalues = line.Split(' ', '\t');

                // For every value in the keyword...
                for (int j = 0; j < keyvalues.Length; j++)
                {
                    keyvalues[j] = keyvalues[j].Replace("\"", ""); // Replace the quotes with nothing, we add quotes ourselves
                    keyvalues[j] = keyvalues[j].Replace("\\", "/"); // Replace backslashes with forward slashes
                    keyvalues[j] = keyvalues[j].ToLower(); // Make all the text lowercase
                }

                // If we haven't already translated the shader...
                if (!variables.HasVariable(VariableType.Shader))
                {
                    // If we can translate the line as a shader...
                    if (TranslateShader(keyvalues[0], out string vmatShader))
                    {
                        // Add a shader to the list of variables in this VMAT
                        variables.Add(new Variable
                        {
                            Key = "shader",
                            Value = vmatShader,
                            Comment = $"// {line}",
                            VmtKeyValue = keyvalues[0],
                            Type = VariableType.Shader,
                            Group = VariableGroup.Shader
                        });

                        // Skip to the next line
                        continue;
                    }
                    else // Error happened translating the shader!
                    {
                        // If the translated shader is specified as an invalid character...
                        if (vmatShader == "InvalidChar")
                        {
                            // Skip to the next line!
                            continue;
                        }

                        // Write the issue and return
                        sw.WriteLine("// FAULT! SHADER FAILED TO TRANSLATE");
                        return false;
                    }
                }

                // If we can translate the current keyword...
                if (TranslateKeyValue(keyvalues[0], out string key, out KeyValueType keyType, out VariableType varType, out VariableGroup varGroup))
                {
                    // Check against different types of values we have

                    // Depending on the type of value we have...
                    switch (keyType)
                    {
                        // If it's unknown...
                        case KeyValueType.Unknown:
                            variables.Add(new Variable
                            {
                                Key = string.Empty,
                                Value = string.Empty,
                                Comment = $"// UNKNOWN! {line}",
                                VmtKeyValue = line,
                                Type = varType,
                                Group = varGroup
                            });
                            break;

                        // If it's a texture...
                        case KeyValueType.Texture:
                            variables.Add(new Variable
                            {
                                Key = key,
                                Value = $"materials/{keyvalues[1]}.{fileExtension}",
                                Comment = $"// {line}",
                                VmtKeyValue = $"{keyvalues[0]} {keyvalues[1]}",
                                Type = varType,
                                Group = varGroup
                            });

                            // Add the texture to the list of VTF files to be converted
                            texturePaths.Add($"materials/{keyvalues[1]}.vtf");
                            break;

                        // If it's a number or string...
                        case KeyValueType.Text:
                        case KeyValueType.Number:
                            variables.Add(new Variable
                            {
                                Key = key,
                                Value = $"{keyvalues[1]}",
                                Comment = $"// {line}",
                                VmtKeyValue = $"{keyvalues[0]} {keyvalues[1]}",
                                Type = varType,
                                Group = varGroup
                            });
                            break;

                        // If it's a Vector2...
                        case KeyValueType.Vector2:
                            variables.Add(new Variable
                            {
                                Key = key,
                                Value = $"[{keyvalues[1]} {keyvalues[2]}]",
                                Comment = $"// {line}",
                                VmtKeyValue = $"{keyvalues[0]} [{keyvalues[1]} {keyvalues[2]}]",
                                Type = VariableType.Vector2,
                                Group = varGroup
                            });
                            break;

                        // If it's a Vector2 with both values being the same...
                        case KeyValueType.SameValueV2:
                            variables.Add(new Variable
                            {
                                Key = key,
                                Value = $"[{keyvalues[1]} {keyvalues[1]}]",
                                Comment = $"// {line}",
                                VmtKeyValue = $"{keyvalues[0]} [{keyvalues[1]} {keyvalues[1]}]",
                                Type = VariableType.Vector2,
                                Group = varGroup
                            });
                            break;

                        // If it's a Vector3...
                        case KeyValueType.Vector3:
                            variables.Add(new Variable
                            {
                                Key = key,
                                Value = $"[{keyvalues[1]} {keyvalues[2]} {keyvalues[3]}]",
                                Comment = $"// {line}",
                                VmtKeyValue = $"{keyvalues[0]} [{keyvalues[1]} {keyvalues[2]} {keyvalues[3]}]",
                                Type = VariableType.Vector3,
                                Group = varGroup
                            });
                            break;
                    }
                }
                else // Otherwise!
                {
                    // Skip to the next line
                    continue;
                }
            }

            // If we are an overlay material, but there's no translucency...
            if (variables.HasVariable(VariableType.Overlay)
                && !variables.HasVariable(VariableType.Alpha))
            {
                variables.Add(new Variable
                {
                    Key = "F_BLEND_MODE",
                    Value = "1",
                    Comment = "// AUTOGENERATED - TRANSLUCENT",
                    VmtKeyValue = "AUTOGENERATED",
                    Type = VariableType.Alpha,
                    Group = VariableGroup.Alpha
                });
            }
            else if (variables.HasVariable(VariableType.Overlay)  // BUT, if we have an overlay material and translucency is enabled...
                && variables.HasVariable(VariableType.Alpha))
            {
                // Remove the alpha variable and replace it with our blend mode
                variables.RemoveVariable(VariableType.Alpha);

                variables.Add(new Variable
                {
                    Key = "F_BLEND_MODE",
                    Value = "1",
                    Comment = "// AUTOGENERATED - TRANSLUCENT",
                    VmtKeyValue = "AUTOGENERATED",
                    Type = VariableType.Alpha,
                    Group = VariableGroup.Alpha
                });
            }

            // If we have a translucency solver, but no translucency texture...
            if (variables.HasVariable(VariableType.Alpha)
            && !variables.HasVariable(VariableType.AlphaTexture))
            {
                variables.Add(new Variable
                {
                    Key = "TextureTranslucency",
                    Value = variables.GetVariable("TextureColor")?.Value.Replace($".{fileExtension}", $"_trans.{fileExtension}") ?? "INVALID",
                    Comment = "// AUTOGENERATED FROM COLOR TEXTURE",
                    VmtKeyValue = "AUTOGENERATED",
                    Type = VariableType.AlphaTexture,
                    Group = VariableGroup.Alpha
                });
            }

            // If we have a normal map or roughness texture, but no PBR enabled...
            if ((variables.HasVariable(VariableType.NormalTexture)
                || variables.HasVariable(VariableType.RoughnessTexture))
                && !variables.HasVariable(VariableType.Specular))
            {
                variables.Add(new Variable
                {
                    Key = "F_SPECULAR",
                    Value = "1",
                    Comment = "// PBR ENABLED",
                    VmtKeyValue = "AUTOGENERATED",
                    Type = VariableType.Specular,
                    Group = VariableGroup.Roughness
                });
            }

            // If we have a self-illum texture, but self-illum is not enabled...
            if (variables.HasVariable(VariableType.SelfIllumTexture)
                && !variables.HasVariable(VariableType.SelfIllum))
            {
                variables.Add(new Variable
                {
                    Key = "F_SELF_ILLUM",
                    Value = "1",
                    Comment = "// SELF-ILLUM ENABLED",
                    VmtKeyValue = "AUTOGENERATED",
                    Type = VariableType.SelfIllum,
                    Group = VariableGroup.SelfIllum
                });
            }
            else if (variables.HasVariable(VariableType.SelfIllum) // BUT, if we have self-illum enabled, but no self-illum texture...
                && !variables.HasVariable(VariableType.SelfIllumTexture))
            {
                variables.Add(new Variable
                {
                    Key = "TextureSelfIllum",
                    Value = variables.GetVariable("TextureColor")!.Value.Replace($".{fileExtension}", $"_selfillum.{fileExtension}"),
                    Comment = "// AUTOGENERATED FROM COLOR TEXTURE",
                    VmtKeyValue = "AUTOGENERATED",
                    Type = VariableType.SelfIllumTexture,
                    Group = VariableGroup.SelfIllum
                });
            }

            // If we have a shader and we're defining ourselves as a decal...
            if (variables.HasVariable(VariableType.Shader)
                && variables.HasVariable(VariableType.Overlay))
            {
                // Change the shader to the static overlay shader
                switch (version)
                {
                    default:
                    case EngineVersion.HLA:
                        variables.GetVariable(VariableType.Shader)!.Value = "vr_static_overlay.vfx";
                        break;

                    case EngineVersion.CS2:
                        variables.GetVariable(VariableType.Shader)!.Value = "static_overlay.vfx";
                        break;

                    case EngineVersion.Sandbox:
                        variables.GetVariable(VariableType.Shader)!.Value = "UNKNOWN!!!";
                        break;
                }
            }

            // Our list of groups and its variables
            Dictionary<VariableGroup, List<Variable>> groups = new();

            // For every variable...
            for (int i = 0; i < variables.Count; i++)
            {
                // Get the current variable
                Variable variable = variables[i];

                // If we don't already have this group...
                if (!groups.ContainsKey(variable.Group))
                {
                    // Add it with a new list of variables, including this one
                    groups.Add(variable.Group, new List<Variable>([variable]));
                }
                else // Otherwise...
                {
                    // Get the group and add this variable to its list of variables
                    groups[variable.Group].Add(variable);
                }

                // Order the groups...
                if (groups.Count > 1)
                {
                    // Sort the groups by their group type
                    groups = groups.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                }
            }

            // For every group...
            foreach (VariableGroup group in groups.Keys)
            {
                // Write some prefix information
                sw.WriteLine($"\t// -- {group} --");

                // For every variable in the group...
                for (int i = 0; i < groups[group].Count; i++)
                {
                    // The current variable in the current group
                    Variable variable = groups[group][i];

                    // Translate different variable types to their VMAT equivalent
                    switch (variable.Type)
                    {
                        default:
                            break;

                        case VariableType.SurfaceProperty:
                            // This is hacky...
                            sw.WriteLine("\n\tSystemAttributes");
                            sw.WriteLine("\t{");

                            sw.WriteLine($"\t\tPhysicsSurfaceProperties \"{TranslateSurfaceProperty(variable.Value)}\"");

                            sw.WriteLine("\t}\n");
                            continue;

                        case VariableType.Cubemap:
                            variable.Value = TranslateCubemap(variable.Value);
                            break;

                        case VariableType.Detail:
                            variable.Value = TranslateDetailMode(variable.Value);
                            break;
                    }

                    // Write its information to the VMAT file!
                    sw.WriteLine($"\t{variable.Key} {(!string.IsNullOrEmpty(variable.Value) ? $"\"{variable.Value}\"" : "")} {variable.Comment}");

                    // Log that we've written the current variable
                    Console.WriteLine($"\"{variable.VmtKeyValue}\" -> \"{variable.Key} {variable.Value}\"");
                }

                // Write a new line, for formatting
                sw.WriteLine("");
            }

            // Closing remarks
            sw.WriteLine("}");
            sw.Close();
        }

        // Log that we're now converting VTFs
        Console.WriteLine($"\nConverting VTFs to {fileExtension.ToUpper()}s...\n");

        // Convert our VTFs using VTFEdit to the preferred file extension
        for (int i = 0; i < texturePaths.Count; i++)
        {
            // Get the directory of the VMT file
            string vmtDir = Path.GetDirectoryName(path)?.Replace('\\', '/') ?? string.Empty;

            // Get the index of "materials"
            int materialsIndex = vmtDir.IndexOf("materials");

            // Remove "materials" and everything after it
            if (materialsIndex >= 0)
            {
                vmtDir = vmtDir.Substring(0, materialsIndex);
            }

            // The current VTF file
            string vtfPath = Path.Combine(vmtDir!, texturePaths[i]);

            // If there's no file at this path...
            if (!File.Exists(vtfPath))
            {
                // Error!
                Console.Error.WriteLine($"Couldn't find VTF file at path \"{vtfPath}\"!");
                return false;
            }

            // If there's not already a converted file with the same name / path...
            if (!File.Exists(Path.ChangeExtension(vtfPath, fileExtension)))
            {
                // Create an instance of VTFCMD to convert the VTF to our desired file extension
                Process? vtfCmd = Process.Start(new ProcessStartInfo()
                {
                    FileName = "VTFCmd.exe",
                    Arguments = $"-file \"{vtfPath}\" -exportformat \"{fileExtension}\" -format \"RGB888\" -silent"
                });

                // Log our conversion!
                Console.WriteLine($"\"{Path.GetFileName(vtfPath)}\" -> \"{Path.GetFileName(Path.ChangeExtension(vtfPath, fileExtension))}\"");
            }
        }

        // Log our success!
        Console.WriteLine("\nSuccessfully translated VMT to VMAT!");
        return true;
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
        // Ignore invalid characters
        if (string.IsNullOrEmpty(vmtShader)
            || vmtShader.Contains("//"))
        {
            vmatShader = "InvalidChar";
            return false;
        }

        switch (vmtShader.ToLower())
        {
            // No valid shader!
            default:
                Console.Error.WriteLine("Invalid shader provided!");
                vmatShader = "Invalid";
                return false;

            // Unlit shader
            case "unlitgeneric":

            // Default shader, used for like 99% of materials
            case "vertexlitgeneric":

            // LightmappedGeneric, used for brushes / static objects
            case "lightmappedgeneric":
                switch (version)
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
                switch (version)
                {
                    // In HL:A it's known as "VR Simple 2way Blend"
                    default:
                    case EngineVersion.HLA:
                        vmatShader = "vr_simple_2way_blend.vfx";
                        break;
                }
                return true;

            // Refracting shader
            // AFAIK does NOT exist in HL:A
            case "refract":
                switch (version)
                {
                    default:
                    case EngineVersion.HLA:
                    case EngineVersion.CS2:
                    case EngineVersion.Sandbox:
                        vmatShader = "unknown";
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
    private static bool TranslateKeyValue(string vmtKey, out string vmatKey, out KeyValueType valType, out VariableType varType, out VariableGroup varGroup)
    {
        // Ignore empty lines, comments, and other invalid characters
        if (string.IsNullOrEmpty(vmtKey)
            || vmtKey == "{" || vmtKey == "}"
            || vmtKey.Contains("//"))
        {
            vmatKey = "Unknown";
            valType = KeyValueType.Unknown;
            varType = VariableType.Unknown;
            varGroup = VariableGroup.Unknown;
            return false;
        }

        switch (vmtKey)
        {
            // If the key isn't recognized
            default:
                Console.Error.WriteLine($"Unknown VMT key encountered: \"{vmtKey}\"");
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
                valType = KeyValueType.Text;
                varType = VariableType.SurfaceProperty;
                varGroup = VariableGroup.Physics;
                return true;

            // Alpha texture
            case "$translucent":
                vmatKey = "F_TRANSLUCENT";
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
                valType = KeyValueType.Text;
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

            // Determines that this is a decal!
            // This should therefore change the shader to e.g. "vr_static_overlay.vfx"
            case "$decal":
                vmatKey = "F_LIT";
                valType = KeyValueType.Number;
                varType = VariableType.Overlay;
                varGroup = VariableGroup.Shader;
                return true;
        }
    }

    /// <summary>
    /// Directly translates a VMT surface property to an equivalent VMAT one.
    /// </summary>
    /// <param name="vmtSurfProp">The surface property from the VMT.</param>
    /// <returns>The VMAT equivalent of the provided VMT surface property, based on which engine version we're translating to.</returns>
    private static string TranslateSurfaceProperty(string vmtSurfProp)
    {
        switch (vmtSurfProp)
        {
            // Default surface property is... Well, default
            default:
            case "default":
                return "default";

            // Default silent surface property is the same as default, but silent! Wow!
            case "default_silent":
                return "default_silent";

            // Disables decals on this material
            case "no_decal":
                return "no_decal";

            // Special material for player controller
            case "player_control_clip":
            case "player":
                return "player";


            // Small items
            // There's no direct translation for this...
            // Just label it as concrete :P
            case "item":

            // Should just be concrete, there's no direct equivalent
            case "boulder":

            // Concrete surface property
            case "concrete":
                switch (version)
                {
                    default:
                    case EngineVersion.HLA:
                    case EngineVersion.CS2:
                        return "prop.concrete";

                    case EngineVersion.Sandbox:
                        return "concrete";
                }

            // Small concrete cinder blocks
            case "concrete_block":
                switch (version)
                {
                    default:
                    case EngineVersion.HLA:
                    case EngineVersion.CS2:
                        return "prop.concrete_block";

                    case EngineVersion.Sandbox:
                        return "concrete";
                }

            // Bricks! Self explanatory
            case "brick":
                switch (version)
                {
                    default:
                    case EngineVersion.HLA:
                    case EngineVersion.CS2:
                        return "prop.brick";

                    case EngineVersion.Sandbox:
                        return "brick";
                }

            // Gravelous grounds
            case "gravel":
                switch (version)
                {
                    default:
                    case EngineVersion.HLA:
                    case EngineVersion.CS2:
                        return "world.gravel";

                    case EngineVersion.Sandbox:
                        return "gravel";
                }

            // Small, solid rocks
            case "rock":
                switch (version)
                {
                    default:
                    case EngineVersion.HLA:
                    case EngineVersion.CS2:
                        return "prop.rock";

                    case EngineVersion.Sandbox:
                        return "rock";
                }

            // Wood ladder, no direct translation, regular wood properties
            case "woodladder":
                switch (version)
                {
                    default:
                    case EngineVersion.HLA:
                    case EngineVersion.CS2:
                        return "prop.wood";

                    case EngineVersion.Sandbox:
                        return "wood";
                }

            // Metal surface property
            case "metal":
                switch (version)
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
    private static string TranslateDetailMode(string vmtDetailMode)
    {
        switch (vmtDetailMode)
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
    private static string TranslateCubemap(string vmtCubemapMode)
    {
        switch (vmtCubemapMode)
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
    private static bool IsValidArg(string input, string validArg)
    {
        return input.Equals(validArg, StringComparison.OrdinalIgnoreCase);
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
        Text,

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