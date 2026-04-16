namespace MyPaint.Core.History
{
    public interface IUndoableCommand
    {
        string Name { get; }
        void Execute();
        void Undo();
    }
}
