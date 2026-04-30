using FluentValidation;
using FootballAI.Application.DTOs;

namespace FootballAI.Application.Validators;

public class VideoUploadValidator : AbstractValidator<VideoUploadDto>
{
    private static readonly string[] AllowedExtensions = { ".mp4", ".avi", ".mov", ".mkv", ".wmv" };

    public VideoUploadValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.MatchDate)
            .NotEmpty().WithMessage("Match date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Match date cannot be in the future.");

        RuleFor(x => x.Venue)
            .MaximumLength(200).WithMessage("Venue must not exceed 200 characters.");

        RuleFor(x => x.HomeTeamId)
            .NotEmpty().WithMessage("Home team is required.");

        RuleFor(x => x.AwayTeamId)
            .NotEmpty().WithMessage("Away team is required.")
            .NotEqual(x => x.HomeTeamId).WithMessage("Home team and away team must be different.");
    }
}
