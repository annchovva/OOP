namespace VectorEditor.Core.Commands
{
    public interface IEditorCommand
    {
        void Execute();   // Выполнить действие
        void Unexecute(); // Отменить действие
    }
}
