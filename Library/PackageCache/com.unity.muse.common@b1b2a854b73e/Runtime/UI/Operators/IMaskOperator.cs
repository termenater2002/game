namespace Unity.Muse.Common
{
    internal interface IMaskOperator : IOperator, IOperatorAddHandler, IOperatorRemoveHandler
    {
        public string GetMask();
        public bool IsClear();
    }
}