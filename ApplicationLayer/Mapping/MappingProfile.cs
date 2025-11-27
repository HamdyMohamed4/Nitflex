using AutoMapper;
using System;
using System.Linq;
using ApplicationLayer.Dtos;
using Domains;
using InfrastructureLayer.UserModels;
using ApplicationLayer.Contract;

namespace ApplicationLayer.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Refresh Tokens & Payment Methods
            CreateMap<TbRefreshTokens, RefreshTokenDto>().ReverseMap();
            CreateMap<TbPaymentMethod, PaymentMethodDto>().ReverseMap();

            // CastMember mappings
            CreateMap<CastMember, CastMemberDto>().ReverseMap();
            CreateMap<CreateCastMemberDto, CastMember>().ReverseMap();
            CreateMap<UpdateCastMemberDto, CastMember>().ReverseMap();






            CreateMap<TVShow, TvShowDto>()
    .ForMember(dest => dest.GenreIds,
        opt => opt.MapFrom(src => src.TVShowGenres.Select(g => g.GenreId).ToList()))
    .ForMember(dest => dest.GenresNames,
        opt => opt.MapFrom(src => src.TVShowGenres
            .Select(g => new GenreDto { Id = g.Genre.Id, Name = g.Genre.Name })
            .ToList()))
    .ForMember(dest => dest.Cast,
        opt => opt.MapFrom(src => src.Castings
            .Where(c => c.CastMember != null)
            .Select(c => new CastDto
            {
                Id = c.CastMember.Id,
                Name = c.CastMember.Name,
                CharacterName = c.CharacterName
            }).ToList()))
    .ForMember(dest => dest.Seasons,
        opt => opt.MapFrom(src => src.Seasons
            .Where(s => s.CurrentState == 1)
            .OrderBy(s => s.SeasonNumber)
            .Select(s => new SeasonDto
            {
                SeasonNumber = s.SeasonNumber,
                Episodes = s.Episodes
                    .Where(e => e.CurrentState == 1)
                    .OrderBy(e => e.EpisodeNumber)
                    .Select(e => new EpisodeDto
                    {
                        EpisodeNumber = e.EpisodeNumber,
                        Title = e.Title,
                    }).ToList()
            }).ToList()));








            // Movie mappings
            CreateMap<Movie, MovieDto>()
                .ForMember(dest => dest.GenreIds,
                    opt => opt.MapFrom(src => src.MovieGenres.Select(g => g.GenreId)))
                .ForMember(dest => dest.GenresNames,
                    opt => opt.MapFrom(src => src.MovieGenres.Select(g => new GenreDto
                    {
                        Id = g.Genre.Id,
                        Name = g.Genre.Name
                    }).ToList()))
                .ForMember(dest => dest.Castings,
                    opt => opt.MapFrom(src => src.Castings.Select(c => new CastDto
                    {
                        Id = c.CastMemberId,
                        Name = c.CastMember.Name,
                        CharacterName = c.CharacterName
                    }).ToList()));

            CreateMap<MovieDto, Movie>()
                .ForMember(dest => dest.MovieGenres,
                    opt => opt.MapFrom(src => src.GenreIds.Select(id => new MovieGenre { GenreId = id })))
                .ForMember(dest => dest.Castings,
                    opt => opt.MapFrom(src => src.Castings.Select(c => new MovieCast
                    {
                        CastMemberId = c.Id,
                        CharacterName = c.CharacterName
                    })));

            // Genre
            CreateMap<Genre, GenreDto>().ReverseMap();
            CreateMap<TVShowGenre, GenreDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Genre.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Genre.Name));

            // TV Show mappings
            CreateMap<TVShow, TvShowDto>()
                .ForMember(dest => dest.GenreIds,
                    opt => opt.MapFrom(src => src.TVShowGenres.Select(g => g.GenreId)))
                .ForMember(dest => dest.GenresNames,
                    opt => opt.MapFrom(src => src.TVShowGenres.Select(g => new GenreDto
                    {
                        Id = g.Genre.Id,
                        Name = g.Genre.Name
                    }).ToList()))
                .ForMember(dest => dest.Cast,
                    opt => opt.MapFrom(src => src.Castings.Select(c => new CastDto
                    {
                        Id = c.CastMemberId,
                        Name = c.CastMember.Name,
                        CharacterName = c.CharacterName
                    }).ToList()))
                .ForMember(dest => dest.Seasons,
                    opt => opt.MapFrom(src => src.Seasons.Where(s => s.CurrentState == 1)
                                                        .OrderBy(s => s.SeasonNumber)
                                                        .ToList()));

            CreateMap<TVShow, TvShowDetailsDto>()
                .ForMember(dest => dest.GenresNames,
                    opt => opt.MapFrom(src => src.TVShowGenres.Select(g => new GenreDto
                    {
                        Id = g.Genre.Id,
                        Name = g.Genre.Name
                    }).ToList()))
                .ForMember(dest => dest.Cast,
                    opt => opt.MapFrom(src => src.Castings.Select(c => new CastDto
                    {
                        Id = c.CastMemberId,
                        Name = c.CastMember.Name,
                        CharacterName = c.CharacterName
                    }).ToList()))
                .ForMember(dest => dest.Seasons,
                    opt => opt.MapFrom(src => src.Seasons.Where(s => s.CurrentState == 1)
                                                        .OrderBy(s => s.SeasonNumber)
                                                        .ToList()));

            CreateMap<CreateTvShowDto, TVShow>().ReverseMap();
            CreateMap<UpdateTvShowDto, TVShow>().ReverseMap();

            // Seasons & Episodes
            CreateMap<Season, SeasonDto>()
                .ForMember(dest => dest.Episodes, opt => opt.MapFrom(src => src.Episodes.Where(e => e.CurrentState == 1).ToList()));
            CreateMap<Season, SeasonWithEpisodesDto>()
                .ForMember(dest => dest.Episodes, opt => opt.MapFrom(src => src.Episodes.Where(e => e.CurrentState == 1).ToList()));
            CreateMap<Episode, EpisodeDto>().ReverseMap();

            CreateMap<CreateSeasonDto, Season>();
            CreateMap<UpdateSeasonDto, Season>();
            CreateMap<CreateEpisodeDto, Episode>().ReverseMap();
            CreateMap<UpdateEpisodeDto, Episode>().ReverseMap();

            // Watchlist
            CreateMap<UserWatchlist, WatchlistItemDto>().ReverseMap();
            CreateMap<AddToWatchlistDto, UserWatchlist>();

            // Ratings
            CreateMap<UserRating, RatingDto>().ReverseMap();
            CreateMap<RateContentDto, UserRating>();

            // OTP
            CreateMap<EmailOtp, OtpDto>().ReverseMap();

            // Subscriptions
            CreateMap<SubscriptionPlan, SubscriptionPlanDto>().ReverseMap();
            CreateMap<CreateSubscriptionPlanDto, SubscriptionPlan>();
            CreateMap<UpdateSubscriptionPlanDto, SubscriptionPlan>();
            CreateMap<UserSubscription, UserSubscriptionDto>().ReverseMap();
            CreateMap<CreateUserSubscriptionDto, UserSubscription>();

            // Users
            CreateMap<ApplicationUser, RegisterDto>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<ApplicationUser, LoginDto>().ReverseMap();
            CreateMap<ApplicationUser, LoginWithOtpDto>().ReverseMap();
            CreateMap<ApplicationUser, UserResultDto>().ReverseMap();
            CreateMap<ApplicationUser, UserDto>().ReverseMap();
            CreateMap<CreateUserDto, ApplicationUser>();
            CreateMap<UpdateUserDto, ApplicationUser>();

            // Profiles
            CreateMap<UserProfile, UserProfileDto>().ReverseMap();

            // User History
            CreateMap<UserHistory, UserHistoryDto>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => (string?)null));
            CreateMap<UserHistoryDto, UserHistory>().ReverseMap();

            // Notifications
            CreateMap<Notification, NotificationDto>().ReverseMap();
            CreateMap<NotificationDto, Notification>().ReverseMap();
        }
    }
}
