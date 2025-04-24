namespace VMT2VMAT;

public static class ListExtensions
{
    /// <summary>
    /// Determines if we have a desired texture type in the list of textures.
    /// </summary>
    /// <typeparam name="T">Should be <see cref="VMATVariable"/>.</typeparam>
    /// <param name="list">This list.</param>
    /// <param name="desiredType">The desired <see cref="VMATVariableType"/> we wish to look for.</param>
    /// <returns><see langword="true"/> if this list features a texture with the desired type, <see langword="false"/> otherwise.</returns>
    public static bool HasVariable<T>( this List<T> list, VMATVariableType desiredType )
        where T : VMATVariable
    {
        foreach ( T item in list )
        {
            if ( item.type == desiredType )
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if we have a variable with the desired keyword in the list of variables.
    /// </summary>
    /// <typeparam name="T">Should be <see cref="VMATVariable"/>.</typeparam>
    /// <param name="list">This list.</param>
    /// <param name="keyword">The keyword of the variable we wish to retrieve.</param>
    /// <returns><see langword="true"/> if this list contains a variable with that keyword, <see langword="false"/> otherwise.</returns>
    public static bool HasVariable<T>( this List<T> list, string keyword )
        where T : VMATVariable
    {
        foreach ( T item in list )
        {
            if ( item.key == keyword )
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets a variable with the desired type from the list.
    /// </summary>
    /// <typeparam name="T">Should be <see cref="VMATVariable"/>.</typeparam>
    /// <param name="list">This list.</param>
    /// <param name="desiredType">The desired <see cref="VMATVariableType"/> we wish to search a variable for.</param>
    /// <returns>The variable of the desired type.</returns>
    public static T? GetVariable<T>( this List<T> list, VMATVariableType desiredType )
        where T : VMATVariable
    {
        foreach ( T item in list )
        {
            if ( item.type == desiredType )
            {
                return item;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a variable with the desired keyword from the list.
    /// </summary>
    /// <typeparam name="T">Should be <see cref="VMATVariable"/>.s</typeparam>
    /// <param name="list">This list.</param>
    /// <param name="keyword">The desired keyword we wish to search a variable for.</param>
    /// <returns>The variable with the desired keyword.</returns>
    public static T? GetVariable<T>( this List<T> list, string keyword )
        where T : VMATVariable
    {
        foreach ( T item in list )
        {
            if ( item.key == keyword )
            {
                return item;
            }
        }

        return null;
    }
}