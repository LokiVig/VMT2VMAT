namespace VMT2VMAT;

public static class ListExtensions
{
    /// <summary>
    /// Determines if we have a desired texture type in the list of textures.
    /// </summary>
    /// <typeparam name="T">Should be <see cref="Variable"/>.</typeparam>
    /// <param name="list">This list.</param>
    /// <param name="desiredType">The desired <see cref="VariableType"/> we wish to look for.</param>
    /// <returns><see langword="true"/> if this list features a texture with the desired type, <see langword="false"/> otherwise.</returns>
    public static bool HasVariable<T>( this List<T> list, VariableType desiredType )
        where T : Variable
    {
        foreach ( T item in list )
        {
            if ( item.Type == desiredType )
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if we have a variable with the desired keyword in the list of variables.
    /// </summary>
    /// <typeparam name="T">Should be <see cref="Variable"/>.</typeparam>
    /// <param name="list">This list.</param>
    /// <param name="keyword">The keyword of the variable we wish to retrieve.</param>
    /// <returns><see langword="true"/> if this list contains a variable with that keyword, <see langword="false"/> otherwise.</returns>
    public static bool HasVariable<T>( this List<T> list, string keyword )
        where T : Variable
    {
        foreach ( T item in list )
        {
            if ( item.Key == keyword )
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets a variable with the desired type from the list.
    /// </summary>
    /// <typeparam name="T">Should be <see cref="Variable"/>.</typeparam>
    /// <param name="list">This list.</param>
    /// <param name="desiredType">The desired <see cref="VariableType"/> we wish to search a variable for.</param>
    /// <returns>The variable of the desired type.</returns>
    public static T? GetVariable<T>( this List<T> list, VariableType desiredType )
        where T : Variable
    {
        foreach ( T item in list )
        {
            if ( item.Type == desiredType )
            {
                return item;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a variable with the desired keyword from the list.
    /// </summary>
    /// <typeparam name="T">Should be <see cref="Variable"/>.</typeparam>
    /// <param name="list">This list.</param>
    /// <param name="keyword">The desired keyword we wish to search a variable for.</param>
    /// <returns>The variable with the desired keyword.</returns>
    public static T? GetVariable<T>( this List<T> list, string keyword )
        where T : Variable
    {
        foreach ( T item in list )
        {
            if ( item.Key == keyword )
            {
                return item;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the list of variables from one group.
    /// </summary>
    /// <typeparam name="T">Should be <see cref="Variable"/>.</typeparam>
    /// <param name="list">This list.</param>
    /// <param name="group">The desired group we wish to get variables from.</param>
    /// <returns>An enumerable list of variables from the desired group.</returns>
    public static List<T>? GetVariablesFromGroup<T>( this List<T> list, VariableGroup group )
        where T : Variable
    {
        List<T> result = (List<T>)list.Where( item => item.Group == group );

        return result.Count() > 0 ? result : null;
    }
}