namespace VMT2VMAT;

/// <summary>
/// A valid VMAT variable.<br/>
/// Holds a key, a value, and a possible comment.
/// </summary>
public class VMATVariable
{
    /// <summary>
    /// The keyword for this variable.<br/>
    /// Can be e.g. TextureColor, TextureRoughness, etc.
    /// </summary>
    public string key;

    /// <summary>
    /// This variable's value.
    /// </summary>
    public string value;

    /// <summary>
    /// Any comment we wish to write.
    /// </summary>
    public string comment;

    /// <summary>
    /// The type of variable this is.
    /// </summary>
    public VMATVariableType type;
}

/// <summary>
/// The different types of VMAT variables that are available to translate.
/// </summary>
public enum VMATVariableType
{
    /// <summary>
    /// Unknown, we don't know what this is.
    /// </summary>
    Unknown,

    /// <summary>
    /// The variable is a shader.
    /// </summary>
    Shader,

    /// <summary>
    /// The variable is a surface property.
    /// </summary>
    SurfaceProperty,

    /// <summary>
    /// The variable defines the fact that PBR is used.
    /// </summary>
    Specular,

    /// <summary>
    /// The variable defines that a detail texture is being used, and which one it is.
    /// </summary>
    Detail,
    
    /// <summary>
    /// The variable defines that we're using cubemaps for reflections, and whether it's none, an in-game cubemap, or an artist cubemap (texture).
    /// </summary>
    Cubemap,

    /// <summary>
    /// The variable defines that we're using the alpha translucency solver.
    /// </summary>
    Alpha,

    /// <summary>
    /// The variable defines that we're using the default translucency solver.
    /// </summary>
    Translucency,

    /// <summary>
    /// The variable is the color texture.
    /// </summary>
    ColorTexture,

    /// <summary>
    /// The variable is the alpha / translucency texture.
    /// </summary>
    AlphaTexture,

    /// <summary>
    /// The variable is the normal map texture.
    /// </summary>
    NormalTexture,

    /// <summary>
    /// The variable is the roughness texture.
    /// </summary>
    RoughnessTexture,

    /// <summary>
    /// The variable is the metallic texture.
    /// </summary>
    MetalnessTexture,

    /// <summary>
    /// The variable is the artist cubemap texture.
    /// </summary>
    CubemapTexture,

    /// <summary>
    /// The variable is the self-illumination texture.
    /// </summary>
    SelfIllumTexture,

    /// <summary>
    /// The variable is the detail texture.
    /// </summary>
    DetailTexture,

    /// <summary>
    /// This variable is a regular number.
    /// </summary>
    Number,

    /// <summary>
    /// This variable holds 2 numbers in one array.
    /// </summary>
    Vector2,

    /// <summary>
    /// This variable holds 3 numbers in one array.
    /// </summary>
    Vector3,

    /// <summary>
    /// This variable holds 4 numbers in one array.
    /// </summary>
    Vector4
}