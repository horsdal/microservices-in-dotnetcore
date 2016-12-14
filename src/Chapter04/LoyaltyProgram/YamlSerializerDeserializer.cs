namespace LoyaltyProgram
{
   using System;
   using System.Collections.Generic;
   using System.IO;
   using Nancy;
   using Nancy.ModelBinding;
   using Nancy.Responses.Negotiation;
   using YamlDotNet.Serialization;

    public class YamlBodyDeserializer : IBodyDeserializer
    {
        public bool CanDeserialize(MediaRange mediaRange, BindingContext context)
          => mediaRange.Subtype.ToString().EndsWith("yaml");

        public object Deserialize(MediaRange mediaRange, Stream bodyStream, BindingContext context)
        {
          var yamlDeserializer = new Deserializer();
          var reader = new StreamReader(bodyStream);
          return yamlDeserializer.Deserialize(reader, context.DestinationType);
        }
    }

    public class YamlBodySerializer : IResponseProcessor
    {
        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get
            {
              yield return new Tuple<string, MediaRange>("yaml", new MediaRange("application/yaml"));
            }
        }

        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
          =>
            requestedMediaRange.Subtype.ToString().EndsWith("yaml") 
            ? new ProcessorMatch { ModelResult = MatchResult.DontCare, RequestedContentTypeResult = MatchResult.NonExactMatch}
            : ProcessorMatch.None;         

        public Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
          =>
            new Response
            {
              Contents = stream =>
              {
                  var yamlSerializer = new Serializer();
                  var streamWriter = new StreamWriter(stream);
                  yamlSerializer.Serialize(streamWriter, model);
                  streamWriter.Flush();
              },
              ContentType = "application/yaml"
            };      
    }
}