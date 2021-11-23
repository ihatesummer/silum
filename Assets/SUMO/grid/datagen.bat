@echo off
set nVehicle=10
set duration=600
netgenerate --grid --grid.number=4 -L=1 --grid.length=100 --output-file=grid.net.xml
randomTrips.py -n grid.net.xml -o flows.xml --begin 0 --end 1 --period 1 --flows %nVehicle%
jtrrouter --route-files=flows.xml --net-file=grid.net.xml --output-file=grid.rou.xml --begin 0 --end %duration% --accept-all-destinations
generateContinuousRerouters.py -n grid.net.xml --end %duration% -o rerouter.add.xml 
echo ^<configuration^> > grid.sumocfg
echo     ^<input^> >> grid.sumocfg
echo         ^<net-file value="grid.net.xml"/^> >> grid.sumocfg
echo         ^<route-files value="grid.rou.xml"/^> >> grid.sumocfg
echo         ^<additional-files value="rerouter.add.xml"/^> >> grid.sumocfg
echo     ^</input^> >> grid.sumocfg
echo     ^<time^> >> grid.sumocfg
echo         ^<begin value="0"/^> >> grid.sumocfg
echo         ^<end value="%duration%"/^> >> grid.sumocfg
echo     ^</time^> >> grid.sumocfg
echo     ^<output^> >> grid.sumocfg
echo         ^<fcd-output value="grid.output.xml"/^> >> grid.sumocfg
echo     ^</output^> >> grid.sumocfg
echo ^</configuration^> >> grid.sumocfg
sumo -c grid.sumocfg --fcd-output mobility_data.xml
python .\xml2csv.py "mobility_data.xml"
echo Data generation completed.
pause