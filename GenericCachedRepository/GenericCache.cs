using System;

namespace GenericCachedRepository
{
    public class GenericCache<TKey, TValue, TIReadRepository, TIWriteRepository>
    {
        private readonly TIReadRepository _primaryReadRepository;
        private readonly TIReadRepository _secondaryReadRepository;
        private readonly TIWriteRepository _primaryWriteRepository;
        private readonly Func<TIReadRepository, TKey, TValue> _getDataFunc;
        private readonly Action<TIWriteRepository, TKey, TValue> _setDataFunc;

        public GenericCache(TIReadRepository primaryReadRepository,
                            TIReadRepository secondaryReadRepository,
                            TIWriteRepository primaryWriteRepository,
                            Func<TIReadRepository, TKey, TValue> getDataFunc,
                            Action<TIWriteRepository, TKey, TValue> setDataFunc)
        {
            _primaryReadRepository = primaryReadRepository;
            _secondaryReadRepository = secondaryReadRepository;
            _primaryWriteRepository = primaryWriteRepository;
            _getDataFunc = getDataFunc;
            _setDataFunc = setDataFunc;
        }

        public virtual TValue GetDataByKey(TKey key)
        {
            var value = GetDataFromPrimaryResource(key);
            if (null == value)
            {
                value = GetDataFromSecondaryResource(key);

                if (value != null)
                    SetDataPrimaryResource(key, value);
            }
            return value;
        }

        private void SetDataPrimaryResource(TKey key, TValue value)
        {
            _setDataFunc.Invoke(_primaryWriteRepository, key, value);
        }

        private TValue GetDataFromPrimaryResource(TKey key)
        {
            return _getDataFunc.Invoke(_primaryReadRepository, key);
        }

        private TValue GetDataFromSecondaryResource(TKey key)
        {
            return _getDataFunc.Invoke(_secondaryReadRepository, key);
        }
    }
}
