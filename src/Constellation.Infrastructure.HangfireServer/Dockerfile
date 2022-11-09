FROM mcr.microsoft.com/dotnet/aspnet:6.0
COPY bin/Release/net6.0/publish/ App/
WORKDIR /App

# Set the proxy details for the build environment
ENV HTTP_PROXY="http://10.136.44.14:3128"
ENV HTTPS_PROXY="http://10.136.44.14:3128"
ENV FTP_PROXY="http://10.136.44.14:3128"
ENV NO_PROXY="127.0.0.1,localhost,10.136.44.*,*.det.nsw.edu.au"

# Reduce the required security level for external connections to allow 
# connection to Sentral server
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/CipherString = DEFAULT@SECLEVEL=2/CipherString = DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf

# Set the locale
## From http://jaredmarkell.com/docker-and-locales/
RUN apt-get clean && apt-get update && apt-get install -y locales
RUN locale-gen en_AU.UTF-8
ENV LANG en_AU.UTF-8
ENV LANGUAGE en_AU:en
ENV LC_ALL en_AU.UTF-8

# Update Timezone settings to correct date format issues
ENV TZ=Australia/Sydney
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

ENTRYPOINT ["dotnet", "Constellation.Infrastructure.HangfireServer.dll"]