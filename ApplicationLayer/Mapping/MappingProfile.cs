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

            // Profile
            CreateMap<UserProfile, UserProfileDto>()
                .ForMember(dest => dest.ProfileId, opt => opt.MapFrom(x => x.Id))
                .ForMember(dest => dest.WatchListItems, opt => opt.MapFrom(x => x.WatchlistItems));

            CreateMap<UserHistory, UserHistoryDto>();
        }
    }
}
