using AutoMapper;
using System;
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

            // Movie
            //CreateMap<Movie, MovieDto>().ReverseMap();
            CreateMap<Movie, GenreMoviesResponseDto>().ReverseMap();
            CreateMap<Movie, MovieSearchFilterDto>().ReverseMap();



            CreateMap<CastMember, CastDto>();



            CreateMap<TVShow, TvShowDetailsDto>()
   .ForMember(dest => dest.GenresNames,
              opt => opt.MapFrom(src => src.TVShowGenres.Select(g => new GenreDto
              {
                  Id = g.Genre.Id,
                  Name = g.Genre.Name
              })))
   .ForMember(dest => dest.Cast,
              opt => opt.MapFrom(src => src.Castings.Select(c => new CastDto
              {
                  Id = c.CastMember.Id,
                  Name = c.CastMember.Name,
                  CharacterName = c.CharacterName
              })))
   .ForMember(dest => dest.Seasons,
              opt => opt.MapFrom(src => src.Seasons
                  .Where(s => s.CurrentState == 1)
                  .OrderBy(s => s.SeasonNumber)
              ));



            //CreateMap<Movie, MovieDto>()
            //    .ForMember(dest => dest.GenreIds,
            //        opt => opt.MapFrom(src => src.MovieGenres.Select(mg => mg.GenreId)))
            //    .ForMember(dest => dest.GenresNames,
            //        opt => opt.MapFrom(src => src.MovieGenres.Select(mg => new GenreDto
            //        {
            //            Id = mg.Genre.Id,
            //            Name = mg.Genre.Name
            //        })))
            //    .ForMember(dest => dest.Castings,
            //        opt => opt.MapFrom(src => src.Castings.Select(c => new CastDto
            //        {
            //            Id = c.CastMember.Id,
            //            Name = c.CastMember.Name,
            //            CharacterName = c.CharacterName
            //        })));





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
        opt => opt.MapFrom(src => src.GenreIds.Select(id => new MovieGenre
        {
            GenreId = id
        })))
    .ForMember(dest => dest.Castings,
        opt => opt.MapFrom(src => src.Castings.Select(c => new MovieCast
        {
            CastMemberId = c.Id,
            CharacterName = c.CharacterName
        })));








            CreateMap<TVShow, TvShowDto>()
    .ForMember(dest => dest.GenreIds,
        opt => opt.MapFrom(src => src.TVShowGenres.Select(g => g.GenreId)))
    .ForMember(dest => dest.GenresNames,
        opt => opt.MapFrom(src => src.TVShowGenres.Select(g => g.Genre)))
    .ForMember(dest => dest.Cast,
        opt => opt.MapFrom(src => src.Castings.Select(c => c.CastMember.Name)))
    .ForMember(dest => dest.Seasons, opt => opt.MapFrom(src => src.Seasons)).ReverseMap();




            CreateMap<Season, SeasonWithEpisodesDto>()
                .ForMember(dest => dest.SeasonNumber, opt => opt.MapFrom(src => src.SeasonNumber))
                .ForMember(dest => dest.Episodes, opt => opt.MapFrom(src => src.Episodes)).ReverseMap();


            // Genre
            CreateMap<Genre, GenreDto>().ReverseMap();


            CreateMap<CreateTvShowDto, TVShow>().ReverseMap();
            CreateMap<UpdateTvShowDto, TVShow>().ReverseMap();
            CreateMap< TVShow, GenreShowsResponseDto>().ReverseMap();



            CreateMap<Season, SeasonDto>().ReverseMap();
            CreateMap<CreateSeasonDto, Season>();
            CreateMap<UpdateSeasonDto, Season>();

            CreateMap<Episode, EpisodeDto>().ReverseMap();
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
                .ForMember(dest => dest.Password, opt => opt.Ignore()) // Password won't be mapped back
                .ReverseMap();
            CreateMap<ApplicationUser, LoginDto>().ReverseMap();
            CreateMap<ApplicationUser, LoginWithOtpDto>().ReverseMap();
            CreateMap<ApplicationUser, UserResultDto>().ReverseMap();
            // Profiles
            CreateMap<UserProfile, UserProfileDto>().ReverseMap();





            // Mapping for UserHistory and UserHistoryDto
            CreateMap<UserHistory, UserHistoryDto>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => string.Empty))  // If you want to map Title separately
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => (string?)null));  // You can customize as needed

            CreateMap<UserHistoryDto, UserHistory>().ReverseMap();
            CreateMap<Notification, NotificationDto>().ReverseMap();


            // Mapping from NotificationDto to Notification (Entity)
            CreateMap<NotificationDto, Notification>().ReverseMap();


            // Users
            CreateMap<ApplicationUser, UserDto>().ReverseMap();
            CreateMap<CreateUserDto, ApplicationUser>();
            CreateMap<UpdateUserDto, ApplicationUser>();

        }
    }
}
