using System.Collections.Generic;
using UnityEngine;

namespace VisualFunctions
{
    [CreateAssetMenu(menuName = "Visual Functions/Variables/ListOf")]
    public class ListOfVariable : Variable<ListOf>
    {
        /**
         * Returns the list of values as a List[TType]
         */
        public List<TType> GetList<TType>()
        {
            return Value.ListValue.Value as List<TType>;
        }
    }
}