#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DeepForestLabs.Services.Connectivity
{
    public sealed class RetryState : IRunnableResult<ResultE>
    {
        private const float timeoutInSeconds = 3;

        [Dependency] readonly string _connectivityCheckUrl = "https://www.google.com";

        public async UniTask<ResultE> Run(CancellationToken token)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(_connectivityCheckUrl))
            {
                UnityWebRequestAsyncOperation op = request.SendWebRequest();
                float timeSinceReceivingData = 0f;

                while (!op.isDone)
                {
                    await UniTask.NextFrame(token);
                    
                    if (request.downloadedBytes > 0)
                    {
                        request.Abort();
                        return ResultE.Success();
                    }
                    
                    timeSinceReceivingData += Time.deltaTime;
                    if (timeSinceReceivingData > timeoutInSeconds)
                    {
                        
                        return ResultE.Error("Timeout");
                    }
                }
                
                switch (request.result)
                {
                    case UnityWebRequest.Result.Success:
                        return ResultE.Success();
                    
                    default:
                        return ResultE.Error(request.result.ToString());
                }
            }
        }
    }
}
#nullable disable
