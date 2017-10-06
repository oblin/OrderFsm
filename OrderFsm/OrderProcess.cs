using OrderFsm.Interfaces;
using Stateless;
using Stateless.Graph;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OrderFsm
{
    public class OrderProcess : IStateEntity
    {
        private enum OrderState { Init, Pricing, Manual, Closed, Reject }
        private enum Trigger { Fail, Success, Assign, Close }

        private OrderState _state;
        private readonly StateMachine<OrderState, Trigger> _machine;
        private readonly StateMachine<OrderState, Trigger>.TriggerWithParameters<string> _assignTrigger;
        private readonly IStateRepository<OrderProcess> _repository;

        public string Assignee { get; set; }
        public bool PricingSuccess { get; set; }
        public bool IsOrderCheckSucess { get; set; }
        public bool ManualSuccess { get; set; }
        private OrderState _State { get => _state;
            set {
                _state = value;
                // 設定資料庫狀態（不允許外部改變）
                this.State = _state.ToString();
            }
        }

        public OrderProcess(string orderId, IStateRepository<OrderProcess> repository)
        {
            this.Key = orderId;
            _State = OrderState.Init;
            // 第二個參數： StateMutator 代表當狀態改變時候，要變更 OrderProcess 的哪一個欄位
            // 通常不用指定，直接使用即可： _machine = new StateMachine<State, Trigger>(_state)
            _machine = new StateMachine<OrderState, Trigger>(() => _State, s => _State = s);

            // 設定 Trigger.Assign 必須要搭配一個 string 的參數
            // 例如：正常的 Trigger 直接使用：bug.Defer() 但設定 TriggerParameter 必須要使用：bug.Assign("Fred");
            _assignTrigger = _machine.SetTriggerParameters<string>(Trigger.Assign);

            // 開始設定 State Machine 流程：
            _machine.Configure(OrderState.Init)
                .Permit(Trigger.Fail, OrderState.Reject)
                .Permit(Trigger.Success, OrderState.Pricing);

            _machine.Configure(OrderState.Pricing)
                //.OnEntry(() => Pricing())
                .Permit(Trigger.Close, OrderState.Closed)
                .Permit(Trigger.Assign, OrderState.Manual);

            _machine.Configure(OrderState.Manual)
                .OnEntryFrom(_assignTrigger, to => OnAssigned(to))
                .PermitReentry(Trigger.Assign)
                .Permit(Trigger.Close, OrderState.Closed)
                .Permit(Trigger.Fail, OrderState.Reject);

            _machine.Configure(OrderState.Closed)
                .OnEntryAsync(() => InsertErpMsnAndNotifyAsync());

            _machine.Configure(OrderState.Reject)
                .OnEntryAsync(() => NotifyFailOnlineAsync());

            _repository = repository;
            _repository.Add(this);
        }

        public void Start(string assignee)
        {
            if (CheckingOrder())
                if (Pricing())
                    Close();
                else
                {
                    if (Manual(assignee))
                        Close();
                    else
                        _machine.FireAsync(Trigger.Fail);
                }
            else
                _machine.FireAsync(Trigger.Fail);
        }

        public bool Pricing()
        {
            this.PricingSuccess = false;
            _machine.Fire(Trigger.Success);

            Console.WriteLine($"Order is Pricing now");

            _repository.Update(this);

            return this.PricingSuccess;
            //if (PricingSuccess)
            //    _machine.Fire<string>(_assignTrigger, this.Assignee);
            //else
            //    _machine.Fire(Trigger.Close);
        }

        public void Close()
        {
            _machine.FireAsync(Trigger.Close);
        }

        public bool Manual(string assignee)
        {
            _machine.Fire(_assignTrigger, assignee);
            this.ManualSuccess = false;

            _repository.Update(this);
            return this.ManualSuccess;
        }

        public string ToDotGraph()
        {
            return UmlDotGraph.Format(_machine.GetInfo());
        }

        private bool CheckingOrder()
        {
            Console.WriteLine($"Order is Checking now");
            this.IsOrderCheckSucess = true;
            return this.IsOrderCheckSucess;
            //var isCheckingSuccess = true;
            //if (isCheckingSuccess)
            //    _machine.Fire(Trigger.Success);
            //else
            //    _machine.Fire(Trigger.Fail);
        }

        private void OnAssigned(string assignee)
        {
            this.Assignee = assignee;

            if (this.Assignee == "ServiceA")
                _machine.Fire(_assignTrigger, "ServiceB");

            Console.WriteLine($"Order is assign to {this.Assignee}");
            //else
            //{
            //    var isCheckingSuccess = true;
            //    if (isCheckingSuccess)
            //        _machine.Fire(Trigger.Close);
            //    else
            //        _machine.Fire(Trigger.Fail);
            //}
        }

        private async Task InsertErpMsnAndNotifyAsync()
        {
            Console.WriteLine("Order is insert to ERP...");
            Console.WriteLine("Order is udpated to MSM...");

            string result = await GetGithubString();
            Console.WriteLine($"Send Success to Order online {result}");
        }

        private async Task NotifyFailOnlineAsync()
        {
            string result = await GetGithubString();
            Console.WriteLine($"Failed Exsit with {result}");
        }

        private static async Task<string> GetGithubString()
        {
            var client = new HttpClient();
            var result = await client.GetAsync("https://api.github.com/orgs/dotnet/repos");
            return result.StatusCode.ToString();
        }
    }
}
