using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationLayer.Dtos;
using Domains;
namespace ApplicationLayer.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        { 

            CreateMap<TbRefreshTokens, RefreshTokenDto>().ReverseMap();
            CreateMap<TbPaymentMethod, PaymentMethodDto>().ReverseMap();

            // Movie mapping (existing)
            CreateMap<Movie, MovieDto>().ReverseMap();

            // Genre
            CreateMap<Genre, GenreDto>().ReverseMap();
            CreateMap<CreateGenreDto, Genre>();
            CreateMap<UpdateGenreDto, Genre>();

            // TvShow, Season, Episode
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

            // Rating
            CreateMap<UserRating, RatingDto>().ReverseMap();
            CreateMap<RateContentDto, UserRating>();

            // Subscriptions
            CreateMap<SubscriptionPlan, SubscriptionPlanDto>().ReverseMap();
            CreateMap<CreateSubscriptionPlanDto, SubscriptionPlan>();
            CreateMap<UpdateSubscriptionPlanDto, SubscriptionPlan>();

            CreateMap<UserSubscription, UserSubscriptionDto>().ReverseMap();
            CreateMap<CreateUserSubscriptionDto, UserSubscription>();
        }

    }
}

