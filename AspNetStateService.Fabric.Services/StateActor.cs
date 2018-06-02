using System;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Core;
using AspNetStateService.Fabric.Interfaces;
using AspNetStateService.Interfaces;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace AspNetStateService.Fabric.Services
{

    /// <summary>
    /// Hosts the <see cref="StateObject"/> in a Service Fabric Actor, using the Actor state.
    /// </summary>
    [StatePersistence(StatePersistence.Volatile)]
    public class StateActor : Actor, IRemindable, IStateActor
    {

        readonly IStateObject state;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="actorService"></param>
        /// <param name="actorId"></param>
        public StateActor(ActorService actorService, ActorId actorId) :
            base(actorService, actorId)
        {
            this.state = new StateObject(new StateActorDataStore(this));
        }

        /// <summary>
        /// Processes a non-exclusive get request.
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public Task<DataResponse> Get()
        {
            return state.Get();
        }

        /// <summary>
        /// Processes an exclusive get request.
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public Task<DataResponse> GetExclusive()
        {
            return state.GetExclusive();
        }

        public Task<Response> Set(uint? cookie, byte[] data, uint? extraFlags, TimeSpan? timeout)
        {
            return state.Set(cookie, data, extraFlags, timeout);
        }

        public Task<Response> ReleaseExclusive(uint cookie)
        {
            return state.ReleaseExclusive(cookie);
        }

        public Task<Response> Remove(uint? cookie)
        {
            return state.Remove(cookie);
        }

        public Task<Response> ResetTimeout()
        {
            return state.ResetTimeout();
        }

        /// <summary>
        /// Tries to get the reminder with the specified name, or returns null.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IActorReminder TryGetReminder(string name)
        {
            try
            {
                return GetReminder("Timeout");
            }
            catch (ReminderNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the expiration reminder.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task SetTimeoutAsync(TimeSpan? timeout)
        {
            // refresh existing reminder
            var reminder = TryGetReminder("Timeout");
            if (timeout != null)
                await RegisterReminderAsync("Timeout", null, timeout.Value, TimeSpan.MaxValue);
            else if (reminder != null)
                await UnregisterReminderAsync(reminder);
        }

        /// <summary>
        /// Invoked when a reminder is received.
        /// </summary>
        /// <param name="reminderName"></param>
        /// <param name="state"></param>
        /// <param name="dueTime"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            // delete actor on expiration
            if (reminderName == "Timeout")
                await ActorService.StateProvider.RemoveActorAsync(this.GetActorId());
        }

    }

}
