using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.ApplicationContainer;
using Vrh.ApplicationContainer.Control.Contract;
using Vrh.ApplicationContainer.Core;

namespace InstanceFactory.Fake
{
    [Export(typeof(IInstanceFactory))]
    public class InstanceFactoryFake : IInstanceFactory
    {
        public string Config
        {
            get
            {
                return "";
            }
        }

        public List<MessageStackEntry> Errors
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public List<MessageStackEntry> Infos
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Dictionary<string, IPlugin> BuildAll()
        {
            return new Dictionary<string, IPlugin>();
        }

        public Dictionary<string, IPlugin> BuildAllFromThis(Type type)
        {
            return new Dictionary<string, IPlugin>();
        }

        public Dictionary<string, IPlugin> BuildAllFromThisUnderThisVersion(Type type, string version)
        {
            return new Dictionary<string, IPlugin>();
        }

        public IPlugin BuildThis(Type type, string name, string version)
        {
            return null;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<InstanceDefinition> GetAllInstance(string pluginType, string version)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PluginDefinition> GetAllPlugin()
        {
            throw new NotImplementedException();
        }
    }
}
