@echo off
set nVehicle=10
set duration=60
randomTrips.py -n road.net.xml -o flows.xml --begin 0 --end 1 --period 1 --flows %nVehicle%
jtrrouter --route-files=flows.xml --net-file=road.net.xml --output-file=road.rou.xml --begin 0 --end %duration% --accept-all-destinations
generateContinuousRerouters.py -n road.net.xml --end %duration% -o rerouter.add.xml 
echo ^<configuration^> > grid.sumocfg
echo     ^<input^> >> grid.sumocfg
echo         ^<net-file value="road.net.xml"/^> >> grid.sumocfg
echo         ^<route-files value="road.rou.xml"/^> >> grid.sumocfg
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
sumo -c grid.sumocfg --fcd-output mobility.xml
echo Data generation completed.
pause