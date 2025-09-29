
using Nop.Services.ScheduleTasks;
namespace Nop.Plugin.Payments.GarantiPos.Services;

public class CheckDeliveryConfirmationTask : IScheduleTask
{
	private readonly ControlManager _controlManager;

	public CheckDeliveryConfirmationTask(ControlManager controlManager)
	{
		_controlManager = controlManager;
	}

	public Task ExecuteAsync()
	{
		return _controlManager.CheckDeliveryPaymentAsync();
	}
}
