using System;
using System.Collections.Generic;
using System.Threading;
using qtLib.Helper;
using qtLib.UIScripts.Base.UIManager;
using UnityEngine;

namespace qtLib.UI.Base
{
    public class CurrentSession
    {
        public List<qtMediator> allMediators = new List<qtMediator>();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public void Add(qtMediator mediator)
        {
            if (mediator != null)
            {
                if (allMediators.Contains(mediator) == false)
                {
                    allMediators.Add(mediator);
                }
            }
        }

        public void Remove(qtMediator mediator)
        {
            if (IsContains(mediator))
            {
                allMediators.Remove(mediator);
            }
        }
        
        private bool IsContains(qtMediator mediator)
            => allMediators.Contains(mediator);

    }
    
    [DefaultExecutionOrder(-49)]
    public class qtUiFlow : MonoBehaviour
    {
        [SerializeField] protected CurrentSession _session;
        private List<qtMediator> _mediatorCreated = new List<qtMediator>();
        public static bool IsBusy = false;

        private void Awake()
        {
            qtDependencyInjection.Add(this);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        public static TMediator Request<TMediator>(bool needRequestNewData = false) where TMediator : qtMediator
        {
            var uiFlow = qtDependencyInjection.Get<qtUiFlow>();
            var mediator = uiFlow.GetOrAdd<TMediator>(needRequestNewData);

            if (uiFlow._session == null)
            {
                uiFlow._session = new CurrentSession();
            }

            uiFlow._session.Add(mediator);
            
            return mediator;
        }

        public static TMediator GetNewest<TMediator>() where TMediator : qtMediator
        {
            var uiFlow = qtDependencyInjection.Get<qtUiFlow>();
            var mediator = uiFlow.Get<TMediator>();
            return mediator;
        }
        
        private TMediator GetOrAdd<TMediator>(bool needRequestNewData = false) where TMediator : qtMediator
        {
            TMediator mediator = Get<TMediator>();
            if (mediator == null || mediator.IsActive())
            {
                mediator = (TMediator)Activator.CreateInstance(typeof(TMediator));

                _mediatorCreated.Add(mediator);
            }

            return mediator;
        }

        private TMediator Get<TMediator>() where TMediator : qtMediator
        {
            TMediator med = null;
            for (var i = _mediatorCreated.Count - 1; i >= 0; i--)
            {
                if (_mediatorCreated[i].GetType() == typeof(TMediator))
                {
                    med = _mediatorCreated[i] as TMediator;
                    break;
                }
            }

            return med;
        }

        private void Start()
        {
            IUIEvent uiEvent = GetComponent<IUIEvent>();
            if (uiEvent == null)
            {
                throw new NotImplementedException("No IUIEvent component found");
            }
            uiEvent.OnStart();
        }
    }
}