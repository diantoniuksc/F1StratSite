window.initCircuitMap = function(lat, lon) {
    if (!window.ol || !document.getElementById('map')) return;
    // Use only provided lat/lon parameters
    var map = new ol.Map({
        target: 'map',
        layers: [
            new ol.layer.Tile({
                source: new ol.source.OSM()
            })
        ],
        view: new ol.View({
            center: ol.proj.fromLonLat([lon, lat]),
            zoom: 15
        })
    });
    var marker = new ol.Feature({
        geometry: new ol.geom.Point(ol.proj.fromLonLat([lon, lat]))
    });
    var vectorSource = new ol.source.Vector({
        features: [marker]
    });
    var markerVectorLayer = new ol.layer.Vector({
        source: vectorSource
    });
    map.addLayer(markerVectorLayer);
};
