using SavePlus_API.Models;
using SavePlus_API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SavePlus_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<SavePlusDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            
            // Register services
            builder.Services.AddScoped<IPatientService, PatientService>();
            builder.Services.AddScoped<IPatientAccountService, PatientAccountService>();
            builder.Services.AddScoped<ITenantService, TenantService>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IOtpService, OtpService>();
            builder.Services.AddScoped<DoctorSearchService>();
            builder.Services.AddScoped<IDoctorService, DoctorService>();

            // Sprint 2 - CarePlan, Measurement, Prescription Services
            builder.Services.AddScoped<ICarePlanService, CarePlanService>();
            builder.Services.AddScoped<IMeasurementService, MeasurementService>();
            builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
            
            // Sprint 3 - Consultation, MedicalRecord, Notification Services
            builder.Services.AddScoped<IConsultationService, ConsultationService>();
            builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            
            // Sprint 4 - Conversation & Message Services (Chat)
            builder.Services.AddScoped<IConversationService, ConversationService>();
            
            // Sprint 5 - Reminder Services (Nhắc nhở)
            builder.Services.AddScoped<IReminderService, ReminderService>();
            
            // Payment Transaction Services
            builder.Services.AddScoped<IPaymentTransactionService, PaymentTransactionService>();
            
            // Service Management
            builder.Services.AddScoped<IServiceService, ServiceService>();
            
            // Tenant Settings Service
            builder.Services.AddScoped<ITenantSettingService, TenantSettingService>();
            
            // Cloudinary Service
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

            
            // Add Memory Cache for OTP storage
            builder.Services.AddMemoryCache();
            
            // Configure JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:SecretKey"] ?? "SavePlus-Super-Secret-Key-For-JWT-Token-Generation-2024")),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization();
            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                // Configure DateOnly for Swagger
                c.MapType<DateOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "string",
                    Format = "date",
                    Example = new Microsoft.OpenApi.Any.OpenApiString("2004-06-30")
                });
                
                c.MapType<DateOnly?>(() => new Microsoft.OpenApi.Models.OpenApiSchema
                {
                    Type = "string",
                    Format = "date",
                    Example = new Microsoft.OpenApi.Any.OpenApiString("2004-06-30"),
                    Nullable = true
                });

                // Configure JWT for Swagger
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "Nhập JWT token vào ô bên dưới (không cần gõ 'Bearer')",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SavePlus API v1");
                        c.RoutePrefix = string.Empty; // 👈 đặt swagger làm trang mặc định
                    });
            }

            // Comment out for development (HTTP only)
            // app.UseHttpsRedirection();

            // Add CORS support
            app.UseCors(policy =>
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod());

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
