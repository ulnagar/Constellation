﻿function DisplayMap(elementid, layerdata) {
    // OpenStreetView tiles
    var tileUrl = 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
    var tileAttribution = 'Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors, <a href="https://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>';

    let layers = layerdata;
    let elementId = elementid;
    console.log(layers);

    var elem = document.getElementById(elementId);
    if (!elem) {
        throw new Error('No element with ID ' + elementId);
    }

    // Initialize map if needed
    if (!elem.map) {
        elem.map = L.map(elementId);
        elem.map.addedLayers = [];
        elem.map.layerGroups = {};
        L.tileLayer(tileUrl, { attribution: tileAttribution }).addTo(elem.map);
    }

    var map = elem.map;
    var markersGroup = new L.featureGroup();

    // iterate over layers and add new layer with included markers for each.
    map.addedLayers = layers.map(l => {
        var group = new L.featureGroup();

        l.markers.map(m => {
            var markerIcon = L.AwesomeMarkers.icon({
                icon: 'school',
                markerColor: m.colour,
                prefix: 'fa'
            });

            var marker = L.marker([m.latitude, m.longitude], { icon: markerIcon }).bindPopup(m.description);
            marker.addTo(markersGroup);
            marker.addTo(group);
        });

        // Add the new layer group (with it's associated name) to the object that becomes the control box
        map.layerGroups[l.name] = group;
        return group;
    });

    map.addedLayers.forEach(layer => {
        layer.addTo(map);
    });

    // Add each layer to the control box so they can be toggled
    L.control.layers(null, map.layerGroups).addTo(map);
    L.control.scale().addTo(map);

    // Auto-fit the view
    map.fitBounds(markersGroup.getBounds()); //.pad(0.3));
};