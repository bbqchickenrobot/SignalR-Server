﻿using System;
using Moq;
using Microsoft.AspNet.SignalR.Infrastructure;
using Xunit;
using Microsoft.AspNet.SignalR.Json;
using Microsoft.AspNet.SignalR.Messaging;
using Newtonsoft.Json;
using Microsoft.AspNet.Logging;

namespace Microsoft.AspNet.SignalR.Tests.Server
{
    public class ConnectionFacts
    {
        [Fact]
        public void SendingCommandObjectSetsCommandOnBus()
        {
            var messageBus = new Mock<IMessageBus>();
            var counters = new Mock<IPerformanceCounterManager>();
            Message message = null;
            messageBus.Setup(m => m.Publish(It.IsAny<Message>())).Returns<Message>(m =>
            {
                message = m;
                return TaskAsyncHelper.Empty;
            });

            var serializer = JsonUtility.CreateDefaultSerializer();
            var loggerFactory = new Mock<ILoggerFactory>();
            var connection = new Connection(messageBus.Object,
                                            serializer,
                                            "signal",
                                            "connectonid",
                                            new[] { "a", "signal", "connectionid" },
                                            new string[] { },
                                            loggerFactory.Object,
                                            new AckHandler(completeAcksOnTimeout: false,
                                                           ackThreshold: TimeSpan.Zero,
                                                           ackInterval: TimeSpan.Zero),
                                            counters.Object,
                                            new Mock<IProtectedData>().Object);

            connection.Send("a", new Command
            {
                CommandType = CommandType.AddToGroup,
                Value = "foo"
            });

            Assert.NotNull(message);
            Assert.True(message.IsCommand);
            var command = serializer.Parse<Command>(message.Value, message.Encoding);
            Assert.Equal(CommandType.AddToGroup, command.CommandType);
            Assert.Equal("foo", command.Value);
        }
    }
}