using AutoMapper;
using System.Linq;
using ApplicationLayer.Dtos;
using Domains;
using InfrastructureLayer.UserModels;

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

            // Genre mappings
            CreateMap<Genre, GenreDto>().ReverseMap();
            CreateMap<TVShowGenre, GenreDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.GenreId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Genre != null ? src.Genre.Name : ""))
                .ReverseMap();

            // Cast mapping
            CreateMap<TvShowCast, CastDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CastMemberId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.CastMember != null ? src.CastMember.Name : ""))
                .ForMember(dest => dest.CharacterName, opt => opt.MapFrom(src => src.CharacterName))
                .ReverseMap();

            CreateMap<MovieCast, CastDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CastMemberId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.CastMember != null ? src.CastMember.Name : ""))
                .ForMember(dest => dest.CharacterName, opt => opt.MapFrom(src => src.CharacterName))
                .ReverseMap();

            // Episode & Season mapping
            CreateMap<Episode, EpisodeDto>().ReverseMap();
            CreateMap<Season, SeasonDto>()
                .ForMember(dest => dest.Episodes, opt => opt.MapFrom(src =>
                    src.Episodes != null
                        ? src.Episodes.Where(e => e.CurrentState == 1).ToList()
                        : new List<Episode>()))
                .ReverseMap();

            CreateMap<Season, SeasonWithEpisodesDto>()
                .ForMember(dest => dest.Episodes, opt => opt.MapFrom(src =>
                    src.Episodes != null
                        ? src.Episodes.Where(e => e.CurrentState == 1).ToList()
                        : new List<Episode>()))
                .ReverseMap();

            // TV Show mappings
            CreateMap<TVShow, TvShowDto>()
                .ForMember(dest => dest.GenreIds,
                    opt => opt.MapFrom(src => src.TVShowGenres.Select(g => g.GenreId).ToList()))
                .ForMember(dest => dest.GenresNames,
                    opt => opt.MapFrom(src => src.TVShowGenres
                        .Where(g => g.Genre != null)
                        .Select(g => new GenreDto { Id = g.Genre.Id, Name = g.Genre.Name ,CurrentState=g.Genre.CurrentState }).ToList()))
                .ForMember(dest => dest.Cast,
                    opt => opt.MapFrom(src => src.Castings
                        .OrderBy(c => c.Id)
                        .Take(10)))
                .ForMember(dest => dest.Seasons,
                    opt => opt.MapFrom(src => src.Seasons
                        .OrderBy(s => s.SeasonNumber)))
                .ReverseMap();

            CreateMap<TVShow, TvShowDetailsDto>()
                .ForMember(dest => dest.GenresNames,
                    opt => opt.MapFrom(src => src.TVShowGenres
                        .Where(g => g.Genre != null)
                        .Select(g => new GenreDto { Id = g.Genre.Id, Name = g.Genre.Name ,CurrentState=g.Genre.CurrentState }).ToList()))
                .ReverseMap();

            CreateMap<CreateTvShowDto, TVShow>().ReverseMap();
            CreateMap<UpdateTvShowDto, TVShow>().ReverseMap();

            // Movie -> DTO
            CreateMap<Movie, MovieDto>()
                .ForMember(dest => dest.GenresNames,
                    opt => opt.MapFrom(src => src.MovieGenres
                        .Where(g => g.Genre != null)
                        .Select(g => new GenreDto
                        {
                            Id = g.Genre.Id,
                            Name = g.Genre.Name,
                            CurrentState = g.Genre.CurrentState

                        }).ToList()))
                .ForMember(dest => dest.Castings, opt => opt.MapFrom(src => src.Castings));


            // DTO -> Movie
            CreateMap<MovieDto, Movie>()
                .ForMember(dest => dest.MovieGenres,
                    opt => opt.MapFrom(src => src.GenresNames.Select(g => new MovieGenre
                    {
                        GenreId = g.Id,
                        CurrentState = 1

                    })))
                .ForMember(dest => dest.Castings,
                    opt => opt.MapFrom(src => src.Castings.Select(c => new MovieCast
                    {
                        CastMemberId = c.Id,
                        CharacterName = c.CharacterName
                    })));


            // Watchlist
            CreateMap<UserWatchlist, WatchlistItemDto>().ReverseMap();
            CreateMap<AddToWatchlistDto, UserWatchlist>().ReverseMap();

            // Ratings
            CreateMap<UserRating, RatingDto>().ReverseMap();
            CreateMap<RateContentDto, UserRating>().ReverseMap();

            // OTP
            CreateMap<EmailOtp, OtpDto>().ReverseMap();

            // Subscriptions
            CreateMap<SubscriptionPlan, SubscriptionPlanDto>().ReverseMap();
            CreateMap<CreateSubscriptionPlanDto, SubscriptionPlan>().ReverseMap();
            CreateMap<UpdateSubscriptionPlanDto, SubscriptionPlan>().ReverseMap();
            CreateMap<UserSubscription, UserSubscriptionDto>().ReverseMap();
            CreateMap<CreateUserSubscriptionDto, UserSubscription>().ReverseMap();

            // Users
            CreateMap<ApplicationUser, RegisterDto>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<ApplicationUser, LoginDto>().ReverseMap();
            CreateMap<ApplicationUser, LoginWithOtpDto>().ReverseMap();
            CreateMap<ApplicationUser, UserResultDto>().ReverseMap();
            CreateMap<ApplicationUser, UserDto>().ReverseMap();
            CreateMap<CreateUserDto, ApplicationUser>().ReverseMap();
            CreateMap<UpdateUserDto, ApplicationUser>().ReverseMap();

            // Profiles
            CreateMap<UserProfile, UserProfileDto>().ReverseMap();

            // User History
            CreateMap<UserHistory, UserHistoryDto>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => (string?)null))
                .ReverseMap();

            // Notifications
            CreateMap<Notification, NotificationDto>().ReverseMap();
        }
    }
}
