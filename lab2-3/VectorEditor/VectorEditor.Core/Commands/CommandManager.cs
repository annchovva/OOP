using System.Collections.Generic;

namespace VectorEditor.Core.Commands
{
    public class CommandManager
    {
        private readonly Stack<IEditorCommand> _undoStack = new Stack<IEditorCommand>();
        private readonly Stack<IEditorCommand> _redoStack = new Stack<IEditorCommand>();

        public void Execute(IEditorCommand command)
        {
            if (command == null)
                return;

            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        /// <summary>
        /// Используется, когда команда уже была выполнена вручную,
        /// но её нужно сохранить в истории для Undo/Redo.
        /// </summary>
        public void RegisterExecutedCommand(IEditorCommand command)
        {
            if (command == null)
                return;

            _undoStack.Push(command);
            _redoStack.Clear();
        }

        public void Undo()
        {
            if (_undoStack.Count == 0)
                return;

            var command = _undoStack.Pop();
            command.Unexecute();
            _redoStack.Push(command);
        }

        public void Redo()
        {
            if (_redoStack.Count == 0)
                return;

            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
    }
}
