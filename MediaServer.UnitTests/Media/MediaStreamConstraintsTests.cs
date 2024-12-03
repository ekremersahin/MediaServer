using MediaServer.Media.Interfaces;
using MediaServer.Media.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
namespace MediaServer.UnitTests.Media
{
    public class MediaStreamConstraintsTests
    {
        [Fact]
        public void CreateVideoConstraints_ShouldSetCorrectProperties()
        {
            // Arrange
            var constraints = new MediaStreamConstraints
            {
                Video = new VideoConstraints
                {
                    Resolution = Resolution.FullHD,
                    FrameRate = FrameRate.High,
                    SourceType = VideoSourceType.Camera
                }
            };

            // Assert
            Assert.Equal(Resolution.FullHD.Width, constraints.Video.Resolution.Width);
            Assert.Equal(Resolution.FullHD.Height, constraints.Video.Resolution.Height);
            Assert.Equal(60, constraints.Video.FrameRate.Fps);
            Assert.Equal(VideoSourceType.Camera, constraints.Video.SourceType);
        }

        [Fact]
        public void CreateAudioConstraints_ShouldSetCorrectProperties()
        {
            // Arrange
            var constraints = new MediaStreamConstraints
            {
                Audio = new AudioConstraints
                {
                    QualityProfile = AudioQualityProfile.VoIP,
                    SourceType = AudioSourceType.Microphone,
                    ProcessingOptions = new AudioProcessingOptions
                    {
                        EchoCancellation = true,
                        NoiseReduction = false
                    }
                }
            };

            // Assert
            Assert.Equal(AudioQualityProfile.VoIP, constraints.Audio.QualityProfile);
            Assert.Equal(AudioSourceType.Microphone, constraints.Audio.SourceType);
            Assert.True(constraints.Audio.ProcessingOptions.EchoCancellation);
            Assert.False(constraints.Audio.ProcessingOptions.NoiseReduction);
        }

        [Fact]
        public void CreateFullMediaStreamConstraints_ShouldGenerateUniqueStreamId()
        {
            // Arrange
            var constraints1 = new MediaStreamConstraints();
            var constraints2 = new MediaStreamConstraints();

            // Assert
            Assert.NotEqual(constraints1.StreamId, constraints2.StreamId);
        }
    }
}
