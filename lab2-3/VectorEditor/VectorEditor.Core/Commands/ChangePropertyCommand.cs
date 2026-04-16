using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VectorEditor.Core;

namespace VectorEditor.Core.Commands
{
    public class ChangePropertyCommand<T> : IEditorCommand
    {
        private readonly List<IShape> _shapes;
        private readonly string _propertyName;
        private readonly T _newValue;
        private readonly List<object?> _oldValues = new List<object?>();

        public ChangePropertyCommand(
            IEnumerable<IShape> shapes,
            Expression<Func<IShape, T>> propertyExpression,
            T newValue,
            List<object?>? explicitOldValues = null)
        {
            if (shapes == null)
                throw new ArgumentNullException(nameof(shapes));

            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            _shapes = shapes.ToList();
            _newValue = newValue;
            _propertyName = GetPropertyName(propertyExpression);

            if (_shapes.Count == 0)
                return;

            if (explicitOldValues != null)
            {
                _oldValues = new List<object?>(explicitOldValues);
            }
            else
            {
                foreach (var shape in _shapes)
                {
                    var prop = shape.GetType().GetProperty(_propertyName);
                    if (prop == null)
                        throw new InvalidOperationException(
                            $"Property '{_propertyName}' not found on type '{shape.GetType().Name}'.");

                    _oldValues.Add(prop.GetValue(shape));
                }
            }
        }

        public void Execute()
        {
            ApplyValue(_newValue);
        }

        public void Unexecute()
        {
            for (int i = 0; i < _shapes.Count && i < _oldValues.Count; i++)
            {
                var prop = _shapes[i].GetType().GetProperty(_propertyName);
                if (prop == null || !prop.CanWrite)
                    continue;

                object? valueToSet = _oldValues[i];

                if (valueToSet != null && !prop.PropertyType.IsAssignableFrom(valueToSet.GetType()))
                {
                    try
                    {
                        valueToSet = Convert.ChangeType(valueToSet, prop.PropertyType);
                    }
                    catch
                    {
                        throw new InvalidOperationException(
                            $"Cannot convert old value for '{_propertyName}' to '{prop.PropertyType.Name}'.");
                    }
                }

                prop.SetValue(_shapes[i], valueToSet);
            }
        }

        private void ApplyValue(T value)
        {
            foreach (var shape in _shapes)
            {
                var prop = shape.GetType().GetProperty(_propertyName);
                if (prop == null || !prop.CanWrite)
                    continue;

                object? boxedValue = value;

                if (boxedValue != null && !prop.PropertyType.IsAssignableFrom(boxedValue.GetType()))
                {
                    try
                    {
                        boxedValue = Convert.ChangeType(boxedValue, prop.PropertyType);
                    }
                    catch
                    {
                        throw new InvalidOperationException(
                            $"Cannot convert value for '{_propertyName}' to '{prop.PropertyType.Name}'.");
                    }
                }

                prop.SetValue(shape, boxedValue);
            }
        }

        private static string GetPropertyName(Expression<Func<IShape, T>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression memberExpression &&
                memberExpression.Member is PropertyInfo propertyInfo)
            {
                if (!propertyInfo.CanWrite)
                    throw new InvalidOperationException($"Property '{propertyInfo.Name}' is read-only.");

                return propertyInfo.Name;
            }

            throw new ArgumentException("Property expression must point to a property.", nameof(propertyExpression));
        }
    }
}
