using AutoMapper;

using SharpC2.API.V1.Requests;
using SharpC2.API.V1.Responses;

using TeamServer.Models;

namespace TeamServer.Mapping
{
    public class CredentialProfiles : Profile
    {
        public CredentialProfiles()
        {
            CreateMap<AddCredentialRecordRequest, CredentialRecord>();
            CreateMap<CredentialRecord, CredentialRecordResponse>();
        }
    }
}