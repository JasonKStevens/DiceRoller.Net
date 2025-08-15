using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DiceRoller.Plugins.Infrastructure.Core;
using DiceRoller.Plugins.Infrastructure.SDK.Attributes;

namespace DiceRoller.Plugins.CallOfCthulhu.Commands
{
    /// <summary>
    /// Handles madness effects for Call of Cthulhu
    /// </summary>
    public class MadnessCommand
    {
        private readonly ILogger _logger;
        private readonly Random _random = new();

        public MadnessCommand(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Execute a madness effect command
        /// </summary>
        [Command("madness", Description = "Roll on the madness table")]
        public async Task<GameSystemCommandResult> ExecuteAsync(
            Dictionary<string, object> parameters,
            GameSystemContext context)
        {
            try
            {
                var madnessType = ExtractStringParameter(parameters, "type", "temporary");
                var customRoll = ExtractIntParameter(parameters, "roll", 0);

                var roll = customRoll > 0 ? customRoll : _random.Next(1, 101);
                var madnessEffect = GetMadnessEffect(roll, madnessType);

                var result = new MadnessResult
                {
                    Roll = roll,
                    MadnessType = madnessType,
                    Effect = madnessEffect.Effect,
                    Duration = madnessEffect.Duration,
                    Description = madnessEffect.Description
                };

                var description = FormatMadnessResult(result);

                _logger.LogDebug("Madness effect rolled: Roll={Roll}, Type={Type}, Effect={Effect}", 
                    result.Roll, result.MadnessType, result.Effect);

                await Task.CompletedTask;
                return new GameSystemCommandResult(true, result, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing madness command");
                return new GameSystemCommandResult(false, null, string.Empty, ex.Message);
            }
        }

        private string ExtractStringParameter(Dictionary<string, object> parameters, string key, string defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value))
                return defaultValue;

            return value.ToString() ?? defaultValue;
        }

        private int ExtractIntParameter(Dictionary<string, object> parameters, string key, int defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value))
                return defaultValue;

            if (int.TryParse(value.ToString(), out var intValue))
                return intValue;

            return defaultValue;
        }

        private MadnessEffect GetMadnessEffect(int roll, string madnessType)
        {
            return madnessType.ToLowerInvariant() switch
            {
                "permanent" => GetPermanentMadnessEffect(roll),
                "temporary" or _ => GetTemporaryMadnessEffect(roll)
            };
        }

        private MadnessEffect GetTemporaryMadnessEffect(int roll)
        {
            return roll switch
            {
                <= 5 => new MadnessEffect("Fainting", "1d4 hours", "Investigator faints and is unconscious"),
                <= 10 => new MadnessEffect("Fleeing", "1d4+1 rounds", "Investigator flees in panic"),
                <= 15 => new MadnessEffect("Physical Hysterics", "1d4+1 rounds", "Uncontrollable screaming and physical outbursts"),
                <= 20 => new MadnessEffect("Emotional Outburst", "1d4+1 rounds", "Uncontrollable crying or laughter"),
                <= 25 => new MadnessEffect("Memory Loss", "1d4 hours", "Temporary amnesia about recent events"),
                <= 30 => new MadnessEffect("Phobia", "1d4+1 hours", "Develops temporary phobia related to the trauma"),
                <= 35 => new MadnessEffect("Mania", "1d4+1 hours", "Develops temporary mania or obsession"),
                <= 40 => new MadnessEffect("Hallucinations", "1d4 hours", "Sees or hears things that aren't there"),
                <= 45 => new MadnessEffect("Paranoia", "1d4+1 hours", "Believes everyone is against them"),
                <= 50 => new MadnessEffect("Catatonia", "1d4 hours", "Becomes unresponsive and motionless"),
                <= 55 => new MadnessEffect("Violence", "1d4+1 rounds", "Attacks nearest person in a frenzy"),
                <= 60 => new MadnessEffect("Specific Phobia", "1d4+1 hours", "Intense fear of specific object or situation"),
                <= 65 => new MadnessEffect("Echopraxia", "1d4 hours", "Compulsively mimics others' actions"),
                <= 70 => new MadnessEffect("Echolalia", "1d4 hours", "Compulsively repeats others' words"),
                <= 75 => new MadnessEffect("Coprophasia", "1d4+1 hours", "Compulsive use of obscene language"),
                <= 80 => new MadnessEffect("Megalomania", "1d4+1 hours", "Believes they have great power or importance"),
                <= 85 => new MadnessEffect("Melancholia", "1d4+1 hours", "Deep depression and withdrawal"),
                <= 90 => new MadnessEffect("Manic Episode", "1d4+1 hours", "Hyperactive and reckless behavior"),
                <= 95 => new MadnessEffect("Ritual Behavior", "1d4 hours", "Compulsively performs repetitive actions"),
                _ => new MadnessEffect("Temporary Insanity", "1d4+1 hours", "Complete break from reality")
            };
        }

        private MadnessEffect GetPermanentMadnessEffect(int roll)
        {
            return roll switch
            {
                <= 10 => new MadnessEffect("Amnesia", "Permanent", "Loses memories of traumatic period"),
                <= 20 => new MadnessEffect("Agoraphobia", "Permanent", "Fear of open or crowded spaces"),
                <= 30 => new MadnessEffect("Claustrophobia", "Permanent", "Fear of enclosed spaces"),
                <= 40 => new MadnessEffect("Paranoid Schizophrenia", "Permanent", "Believes in elaborate conspiracies"),
                <= 50 => new MadnessEffect("Multiple Personality", "Permanent", "Develops alternate personalities"),
                <= 60 => new MadnessEffect("Catatonia", "Permanent", "Periodic unresponsive states"),
                <= 70 => new MadnessEffect("Manic-Depression", "Permanent", "Alternates between mania and depression"),
                <= 80 => new MadnessEffect("Anxiety Disorder", "Permanent", "Chronic overwhelming anxiety"),
                <= 90 => new MadnessEffect("Conversion Disorder", "Permanent", "Physical symptoms with no medical cause"),
                _ => new MadnessEffect("Complete Insanity", "Permanent", "Total break from reality - becomes NPC")
            };
        }

        private string FormatMadnessResult(MadnessResult result)
        {
            var typeText = result.MadnessType.ToUpperInvariant();
            
            var description = $"**{typeText} MADNESS** (Roll: {result.Roll})\n" +
                             $"**Effect:** {result.Effect}\n" +
                             $"**Duration:** {result.Duration}";

            if (!string.IsNullOrEmpty(result.Description))
            {
                description += $"\n**Description:** {result.Description}";
            }

            if (result.MadnessType.ToLowerInvariant() == "permanent")
            {
                description += "\n⚠️ **This is a permanent condition that will affect the investigator indefinitely.**";
            }

            return description;
        }
    }

    /// <summary>
    /// A madness effect from the tables
    /// </summary>
    public class MadnessEffect
    {
        public string Effect { get; set; }
        public string Duration { get; set; }
        public string Description { get; set; }

        public MadnessEffect(string effect, string duration, string description)
        {
            Effect = effect ?? throw new ArgumentNullException(nameof(effect));
            Duration = duration ?? throw new ArgumentNullException(nameof(duration));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }
    }

    /// <summary>
    /// Result of a madness roll
    /// </summary>
    public class MadnessResult
    {
        public int Roll { get; set; }
        public string MadnessType { get; set; } = string.Empty;
        public string Effect { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}