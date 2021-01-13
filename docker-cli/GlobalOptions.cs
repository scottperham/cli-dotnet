using cli_dotnet;

namespace docker_cli
{
    public class GlobalOptions
    {
        [GlobalOption(longForm: "config", helpText:"Location of client config files")]
        public string Config { get; set; }
        [GlobalOption(shortForm:'c', longForm: "context", helpText: "Name of the context to use the connect to the daemon (overrides DOCKER_HOST env var and default context set with \"docker context use\")")]
        public string Context { get; set; }
        [GlobalOption(shortForm: 'D', longForm: "debug", helpText: "Enable debug mode")]
        public bool Debug { get; set; }
        [GlobalOption(shortForm: 'H', longForm: "host", helpText: "Daemon socket(s) to connect to")]
        public string[] Hosts { get; set; }
        [GlobalOption(shortForm: 'l', longForm: "log-level", helpText: "Set the logging level")]
        public LogLevel LogLevel { get; set; }
        [GlobalOption(longForm: "tls", helpText: "Use TLS; implied by --tlsverify")]
        public bool Tls { get; set; }
        [GlobalOption(longForm: "tlscacert", helpText: "Trust certs signed only by this CA")]
        public string TlsCaCert { get; set; }
        [GlobalOption(longForm: "tlscert", helpText: "Path to TLS certificate file")]
        public string TlsCert { get; set; }
        [GlobalOption(longForm: "tlskey", helpText: "Path to TLS key file")]
        public string TlsKey { get; set; }
        [GlobalOption(longForm: "tlsverify", helpText: "Use TLS and verify the remote")]
        public bool TlsVerify { get; set; }
    }
}
