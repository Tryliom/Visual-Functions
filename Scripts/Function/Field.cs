using System;
using System.Collections.Generic;
using UnityEngine;

namespace TryliomFunctions
{
    [Serializable]
    public class Field
    {
        public string FieldName;
        [SerializeReference] public IValue Value;
        public List<SerializableSystemType> SupportedTypes = new();

        public bool InEdition;
        public string EditValue;

#if UNITY_EDITOR
        public bool AcceptAnyMethod;

        /**
         * Use this constructor if you want to support a specific type
         */
        public Field(string name, Type type)
        {
            FieldName = name;
            Value = (IValue)Activator.CreateInstance(ReferenceUtility.GetReferenceType(type));
        }

        /**
         * Use this constructor if you want to support specific Reference types
         */
        public Field(string name, SerializableSystemType[] supportedTypes)
        {
            FieldName = name;

            foreach (var type in supportedTypes) SupportedTypes.Add(ReferenceUtility.GetReferenceType(type.SystemType));
        }

        /**
         * Use this constructor if you want to support all Reference types
         */
        public Field(string name)
        {
            FieldName = name;

            foreach (var supportedType in ReferenceUtility.GetAllReferenceTypes()) SupportedTypes.Add(supportedType);
        }

        public Field Clone()
        {
            return MemberwiseClone() as Field;
        }

        /**
         * Allow to display a button to show all methods from any static class available in the project
         */
        public Field AllowAnyMethod()
        {
            AcceptAnyMethod = true;
            return this;
        }
#endif
    }
}