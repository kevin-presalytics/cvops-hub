using lib.models.db;
using lib.models.dto;
using lib.models;
using System.Threading.Tasks;
using System;
using Serilog;
using Microsoft.EntityFrameworkCore;


namespace lib.services
{
    public interface IDeploymentService : IDisposable
    {
        Task<Deployment> CreateDeployment(DeploymentCreatedPayload payload); 
        Task<Deployment> UpdateDeployment(Deployment updatedDeployment);
        Task DeleteDeployment(Guid DeploymentId);  
    }

    public class DeploymentService : IDeploymentService
    {
        CvopsDbContext _context;
        ILogger _logger;
        public DeploymentService(
            ILogger logger,
            IDbContextFactory<CvopsDbContext> contextFactory
        ) {
            _context = contextFactory.CreateDbContext();
            _logger = logger;
        }

        public Task<Deployment> CreateDeployment(DeploymentCreatedPayload payload)
        {
            throw new NotImplementedException();   
        }

        public Task<Deployment> UpdateDeployment(Deployment deployment)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDeployment(Guid DeploymentId)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
        
    }
}