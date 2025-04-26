namespace VMT2VMAT;

/// <summary>
/// A valid VMAT variable.<br/>
/// Holds a key, a value, and a possible comment.<br/>
/// It is organized by type and group in the VMAT file itself.
/// </summary>
public class Variable
{
    /// <summary>
    /// The keyword for this variable.<br/>
    /// Can be e.g. TextureColor, TextureRoughness, etc.
    /// </summary>
    public string Key = string.Empty;

    /// <summary>
    /// This variable's value.
    /// </summary>
    public string Value = string.Empty;

    /// <summary>
    /// Any comment we wish to write.
    /// </summary>
    public string Comment = string.Empty;

    /// <summary>
    /// The original VMT keyvalue for this variable.
    /// </summary>
    public string VmtKeyValue = string.Empty;

    /// <summary>
    /// The type of variable this is.
    /// </summary>
    public VariableType Type = VariableType.Unknown;

    /// <summary>
    /// The group this variable belongs to.
    /// </summary>
    public VariableGroup Group = VariableGroup.Unknown;
}

/// <summary>
/// The different groups of variables that we can have.
/// </summary>
public enum VariableGroup
{
    /// <summary>
    /// A bit more general of a group.
    /// </summary>
    Unknown,

    /// <summary>
    /// Shaders are automatically in this group.
    /// </summary>
    Shader,

    /// <summary>
    /// The color variables should be in this group.
    /// </summary>
    Color,

    /// <summary>
    /// Translucent or alpha variables should be in this group.
    /// </summary>
    Alpha,

    /// <summary>
    /// The normal variables hsould be in this group.
    /// </summary>
    Normal,

    /// <summary>
    /// The roughness variables should be in this group.
    /// </summary>
    Roughness,

    /// <summary>
    /// The metalness variables should be in this group.
    /// </summary>
    Metalness,

    /// <summary>
    /// The ambient occlusion variables should be in this group.
    /// </summary>
    AmbientOcclusion,

    /// <summary>
    /// The detail variables should be in this group.
    /// </summary>
    Detail,

    /// <summary>
    /// The self-illum variables should be in this group.
    /// </summary>
    SelfIllum,

    /// <summary>
    /// Things that have to do with defining this material's physical properties.
    /// </summary>
    Physics,
}

/// <summary>
/// The different types of VMAT variables that are available to translate.
/// </summary>
public enum VariableType
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
    /// The variable defines that we're using self-illum.
    /// </summary>
    SelfIllum,

    /// <summary>
    /// The variable defines this material as an overlay.
    /// </summary>
    Overlay,

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
    /// The variable is the ambient occlusion texture.
    /// </summary>
    AOTexture,
    
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