using AutoMapper;
using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using InfrastructureLayer.Contracts;
using InfrastructureLayer.Repositories;
using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services
{
    public class PaymentMethodsService : BaseService<TbPaymentMethod,PaymentMethodDto>,IPaymentMethods
    {
        public PaymentMethodsService(IGenericRepository<TbPaymentMethod> repo,IMapper mapper,
             IUserService userService) : base(repo,mapper, userService)
        {

        }
    }
}
