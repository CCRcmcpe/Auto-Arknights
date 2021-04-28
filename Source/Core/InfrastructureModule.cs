using System;

namespace REVUnit.AutoArknights.Core
{
    public class InfrastructureModule
    {
        private readonly Interactor _i;

        internal InfrastructureModule(Interactor interactor)
        {
            _i = interactor;
        }

        public void CollectCreditPoints()
        {
            throw new NotImplementedException();
        }

        public void Collect()
        {
            throw new NotImplementedException();
        }
    }
}