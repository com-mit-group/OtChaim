using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using FluentAssertions;
using OtChaim.Application.Common;
using OtChaim.Application.EmergencyEvents.Commands;
using OtChaim.Presentation.MAUI.ViewModels.Tool;

namespace OtChaim.Application.Tests.ViewModels;

public class EmergencyCreationViewModelTests
{
    [Test]
    public async Task CreateEmergencyCommand_WhenEmailDisabled_DoesNotSetEmailFlag()
    {
        var handler = new RecordingStartEmergencyHandler();
        var viewModel = new EmergencyCreationViewModel(handler);

        viewModel.ToggleEmailCommand.Execute(null);

        await viewModel.CreateEmergencyCommand.ExecuteAsync(null);

        handler.LastCommand.Should().NotBeNull();
        handler.LastCommand!.Attachments.Should().NotBeNull();
        handler.LastCommand.Attachments!.SendEmail.Should().BeFalse();
        handler.LastCommand.Attachments.SendSms.Should().BeTrue();
        handler.LastCommand.Attachments.SendMessenger.Should().BeFalse();
    }

    [Test]
    public async Task CreateEmergencyCommand_WhenSmsDisabled_DoesNotSetSmsFlag()
    {
        var handler = new RecordingStartEmergencyHandler();
        var viewModel = new EmergencyCreationViewModel(handler);

        viewModel.ToggleSmsCommand.Execute(null);

        await viewModel.CreateEmergencyCommand.ExecuteAsync(null);

        handler.LastCommand.Should().NotBeNull();
        handler.LastCommand!.Attachments.Should().NotBeNull();
        handler.LastCommand.Attachments!.SendEmail.Should().BeTrue();
        handler.LastCommand.Attachments.SendSms.Should().BeFalse();
        handler.LastCommand.Attachments.SendMessenger.Should().BeFalse();
    }

    [Test]
    public async Task CreateEmergencyCommand_WhenMessengerDisabled_DoesNotSetMessengerFlag()
    {
        var handler = new RecordingStartEmergencyHandler();
        var viewModel = new EmergencyCreationViewModel(handler);

        viewModel.ToggleMessengerCommand.Execute(null); // enable messenger
        viewModel.ToggleMessengerCommand.Execute(null); // disable messenger again

        await viewModel.CreateEmergencyCommand.ExecuteAsync(null);

        handler.LastCommand.Should().NotBeNull();
        handler.LastCommand!.Attachments.Should().NotBeNull();
        handler.LastCommand.Attachments!.SendEmail.Should().BeTrue();
        handler.LastCommand.Attachments.SendSms.Should().BeTrue();
        handler.LastCommand.Attachments.SendMessenger.Should().BeFalse();
    }

    [Test]
    public void CancelCommand_ResetsContactPreferences()
    {
        var handler = new RecordingStartEmergencyHandler();
        var viewModel = new EmergencyCreationViewModel(handler);

        viewModel.ToggleEmailCommand.Execute(null);
        viewModel.ToggleSmsCommand.Execute(null);
        viewModel.ToggleMessengerCommand.Execute(null);

        viewModel.CancelCommand.Execute(null);

        viewModel.IsEmailSelected.Should().BeTrue();
        viewModel.SendEmail.Should().BeTrue();
        viewModel.IsSmsSelected.Should().BeTrue();
        viewModel.SendSms.Should().BeTrue();
        viewModel.IsMessengerSelected.Should().BeFalse();
        viewModel.SendMessenger.Should().BeFalse();
    }

    private sealed class RecordingStartEmergencyHandler : ICommandHandler<StartEmergency>
    {
        public StartEmergency? LastCommand { get; private set; }

        public Task Handle(StartEmergency command, CancellationToken cancellationToken)
        {
            LastCommand = command;
            return Task.CompletedTask;
        }
    }
}
