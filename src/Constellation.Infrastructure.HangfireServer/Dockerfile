FROM mcr.microsoft.com/dotnet/aspnet:6.0
COPY bin/Release/net6.0/publish/ App/
WORKDIR /App

# Reduce the required security level for external connections to allow 
# connection to Sentral server
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/CipherString = DEFAULT@SECLEVEL=2/CipherString = DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf

# Update Timezone settings to correct date format issues
ENV TZ=Australia/Sydney
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

ENTRYPOINT ["dotnet", "Constellation.Infrastructure.HangfireServer.dll"]