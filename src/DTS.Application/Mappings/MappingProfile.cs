using AutoMapper;
using DTS.Application.DTOs;
using DTS.Domain.Entities;
using DTS.Domain.Enums;

namespace DTS.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<AppUser, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore());

        // Project mappings
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TotalDefects, opt => opt.MapFrom(src => src.Defects.Count))
            .ForMember(dest => dest.OpenDefects, opt => opt.MapFrom(src => src.Defects.Count(d => d.Status != DefectStatus.Closed)))
            .ForMember(dest => dest.ClosedDefects, opt => opt.MapFrom(src => src.Defects.Count(d => d.Status == DefectStatus.Closed)));

        CreateMap<ProjectCreateDto, Project>();
        CreateMap<ProjectUpdateDto, Project>();

        // ProjectMember mappings
        CreateMap<ProjectMember, ProjectMemberDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));

        // Defect mappings
        CreateMap<Defect, DefectDto>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.Name))
            .ForMember(dest => dest.SeverityName, opt => opt.MapFrom(src => src.Severity.ToString()))
            .ForMember(dest => dest.PriorityName, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedTo != null ? src.AssignedTo.FullName : null))
            .ForMember(dest => dest.ReportedByName, opt => opt.MapFrom(src => src.ReportedBy.FullName))
            .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count))
            .ForMember(dest => dest.AttachmentCount, opt => opt.MapFrom(src => src.Attachments.Count));

        CreateMap<DefectCreateDto, Defect>();
        CreateMap<DefectUpdateDto, Defect>();

        // Comment mappings
        CreateMap<DefectComment, DefectCommentDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName));

        // Attachment mappings
        CreateMap<DefectAttachment, DefectAttachmentDto>()
            .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src => src.UploadedBy.FullName));

        // Notification mappings
        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()));

        // AuditLog mappings
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName));
    }
}
