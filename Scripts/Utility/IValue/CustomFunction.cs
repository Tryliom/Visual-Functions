using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualFunctions
{
    /**
     * Used to define a custom value that can be evaluated using a formula. Can be any type.
     */
    [Serializable]
    public class CustomFunction : IValue
    {
        public List<Field> Inputs = new ();
        // Limited to 1 output, if not defined, it's considered a void function
        public List<Field> Outputs = new ();
        public Functions Function = new Functions().DisableGlobalVariables();
        
#if UNITY_EDITOR
        public bool FoldoutOpen = true;
        public bool InputFoldoutOpen = true;
        public bool OutputFoldoutOpen = true;
#endif
        
        private List<IVariable> _variables = new ();
        private List<object> _defaultValues = new ();

        public object Value { get; set; }

        public Type Type => typeof(CustomFunction);
        
        private IValue _defaultReturnValue;
        
        public IValue Evaluate(List<object> inputs, List<IVariable> variables)
        {
            _defaultValues.Capacity = Inputs.Count;
            
            for (var i = 0; i < inputs.Count; i++)
            {
                if (i < Inputs.Count)
                {
                    _defaultValues.Add(Inputs[i].Value.Value);
                    Inputs[i].Value.Value = ExpressionUtility.ConvertTo(inputs[i], Inputs[i].Value.Type);
                }
            }
            
            _variables.Clear();
            _variables.Capacity = Inputs.Count + Outputs.Count + variables.Count;
            
            _variables.AddRange(Inputs);
            _variables.AddRange(Outputs);
            _variables.AddRange(variables);
            
            Function.Invoke(_variables);

            for (var i = 0; i < inputs.Count; i++)
            {
                if (i < Inputs.Count)
                {
                    Inputs[i].Value.Value = _defaultValues[i];
                }
            }
            
            _defaultValues.Clear();
            
            return Outputs.Count > 0 ? Outputs[0].Value : _defaultReturnValue ??= new TempIValue(true);
        }

        public IValue Clone()
        {
            return new CustomFunction
            {
                Inputs = Inputs.Select(input => input.Clone()).ToList(),
                Outputs = Outputs.Select(output => output.Clone()).ToList(),
                Function = Function.Clone(),
                Value = Value
            };
        }
    }
}