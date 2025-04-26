namespace VMT2VMAT;

public static class ListExtensions
{
    /// <summary>
    /// Determines if we have a desired texture type in the list of textures.
    /// </summary>
    /// <typeparam name="T">Should be <see cref="Variable"/>.</typeparam>
    /// <param name="list">This list.</param>
    /// <param name="varType">The desired <see cref="VariableType"/> we wish to look for.</param>
    /// <returns><see langword="true"/> if this list features a texture with the desired type, <see langword="false"/> otherwise.</returns>
    public static bool HasVariable<T>( this List<T> list, VariableType varType )
        where T : Variable
    {
        foreach ( T item in list )
        {
            if ( item.Type == varType )
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
    /// <param name="key">The keyword of the variable we wish to retrieve.</param>
    /// <returns><see langword="true"/> if this list contains a variable with that keyword, <see langword="false"/> otherwise.</returns>
    public static bool HasVariable<T>( this List<T> list, string key )
        where T : Variable
    {
        foreach ( T item in list )
        {
            if ( item.Key == key )
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
    /// <param name="varType">The desired <see cref="VariableType"/> we wish to search a variable for.</param>
    /// <returns>The variable of the desired type.</returns>
    public static T? GetVariable<T>( this List<T> list, VariableType varType )
        where T : Variable
    {
        foreach ( T item in list )
        {
            if ( item.Type == varType )
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
    /// <param name="key">The desired keyword we wish to search a variable for.</param>
    /// <returns>The variable with the desired keyword.</returns>
    public static T? GetVariable<T>( this List<T> list, string key )
        where T : Variable
    {
        foreach ( T item in list )
        {
            if ( item.Key == key )
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
    public static IEnumerable<T>? GetVariablesFromGroup<T>( this List<T> list, VariableGroup group )
        where T : Variable
    {
        IEnumerable<T> result = list.Where( item => item.Group == group );

        return result.Count() > 0 ? result : null;
    }

    /// <summary>
    /// Remove the variable of the specified type.
    /// </summary>
    /// <typeparam name="T">Should be <see cref="Variable"/>.</typeparam>
    /// <param name="list">This list.</param>
    /// <param name="varType">The type of the variable we should remove.</param>
    public static void RemoveVariable<T>(this List<T> list, VariableType varType)
        where T : Variable
    {
        // If this variable doesn't exist...
        if (!HasVariable(list, varType))
        {
            // We can't do anything!
            Console.WriteLine($"Variable of type \"{varType}\" doesn't exist in this list.");
            return;
        }
        else // Otherwise...
        {
            // Remove the variable of the specified type
            list.Remove(GetVariable(list, varType)!);
        }
    }

    /// <summary>
    /// Remove the variable of the specified key.
    /// </summary>
    /// <typeparam name="T">Should be <see cref="Variable"/>.</typeparam>
    /// <param name="list">This list.</param>
    /// <param name="key">The key of the variable we should remove.</param>
    public static void RemoveVariable<T>(this List<T> list, string key)
        where T : Variable
    {
        // If this variable doesn't exist...
        if (!HasVariable(list, key))
        {
            // We can't do anything!
            Console.WriteLine($"Variable with the key \"{key}\" doesn't exist in this list.");
            return;
        }
        else // Otherwise...
        {
            // Remove the variable of the specified type
            list.Remove(GetVariable(list, key)!);
        }
    }
}