using System.Collections.Generic;

namespace MyPaint.Core.History
{
    public class HistoryManager
    {
        private readonly Stack<IUndoableCommand> _undo = new();
        private readonly Stack<IUndoableCommand> _redo = new();

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        public void Execute(IUndoableCommand command)
        {
            command.Execute();
            _undo.Push(command);
            _redo.Clear();
        }

        public void Undo()
        {
            if (!CanUndo) return;
            var cmd = _undo.Pop();
            cmd.Undo();
            _redo.Push(cmd);
        }

        public void Redo()
        {
            if (!CanRedo) return;
            var cmd = _redo.Pop();
            cmd.Execute();
            _undo.Push(cmd);
        }

        public void ClearRedo() => _redo.Clear();
    }
}
