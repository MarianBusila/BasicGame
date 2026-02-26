using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AccountManager : MonoBehaviour
{
    public struct LoginResult
    {
        public bool Success;
        public string Id;
        public string AccessToken;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task<LoginResult> SignInAnonymously()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            return new LoginResult
            {
                Success = true,
                Id = AuthenticationService.Instance.PlayerId,
                AccessToken = AuthenticationService.Instance.AccessToken
            };
        }
        catch (RequestFailedException e)
        {
            Debug.LogError($"Sign in anonymously failed with error code: {e.ErrorCode}");
        }

        return new LoginResult
        {
            Success = false,
            Id = default,
            AccessToken = default
        };
    }
}