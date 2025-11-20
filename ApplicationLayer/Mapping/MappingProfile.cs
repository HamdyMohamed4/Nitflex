using AutoMapper;
using System;
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

            // Movie
            CreateMap<Movie, MovieDto>().ReverseMap();

            // Genre
            CreateMap<Genre, GenreDto>().ReverseMap();
            CreateMap<CreateGenreDto, Genre>();
            CreateMap<UpdateGenreDto, Genre>();

            // TV Shows, Seasons, Episodes
            CreateMap<TVShow, TvShowDto>().ReverseMap();
            CreateMap<TVShow, TvShowDetailsDto>().ReverseMap();
            CreateMap<CreateTvShowDto, TVShow>();
            CreateMap<UpdateTvShowDto, TVShow>();

            CreateMap<Season, SeasonDto>().ReverseMap();
            CreateMap<CreateSeasonDto, Season>();
            CreateMap<UpdateSeasonDto, Season>();

            CreateMap<Episode, EpisodeDto>().ReverseMap();
            CreateMap<CreateEpisodeDto, Episode>();
            CreateMap<UpdateEpisodeDto, Episode>();

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
                .ForMember(dest => dest.Password, opt => opt.Ignore()) // Password won't be mapped back
                .ReverseMap();
            CreateMap<ApplicationUser, LoginDto>().ReverseMap();
            CreateMap<ApplicationUser, LoginWithOtpDto>().ReverseMap();
            CreateMap<ApplicationUser, UserResultDto>().ReverseMap();




            // Mapping for UserHistory and UserHistoryDto
            CreateMap<UserHistory, UserHistoryDto>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => string.Empty))  // If you want to map Title separately
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => (string?)null));  // You can customize as needed

            CreateMap<UserHistoryDto, UserHistory>().ReverseMap();
            CreateMap<Notification, NotificationDto>().ReverseMap();


            // Mapping from NotificationDto to Notification (Entity)
            CreateMap<NotificationDto, Notification>().ReverseMap();

        }
    }
}
